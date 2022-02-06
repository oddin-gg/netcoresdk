using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Sessions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP
{
    internal class AmqpClient : DispatcherBase, IAmqpClient
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(AmqpClient));

        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IApiClient _apiClient;
        private readonly EventHandler<CallbackExceptionEventArgs> _onCallbackException;
        private readonly EventHandler<ShutdownEventArgs> _onConnectionShutdown;
        private readonly IExchangeNameProvider _exchangeNameProvider;
        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;

        public event EventHandler<UnparsableMessageEventArgs> UnparsableMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<alive>> AliveMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<snapshot_complete>> SnapshotCompleteMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<odds_change>> OddsChangeMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<bet_stop>> BetStopMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<bet_settlement>> BetSettlementMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<bet_cancel>> BetCancelMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<fixture_change>> FixtureChangeMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<MessageProcessingEventArgs>> MessageProcessingStarted;
        public event EventHandler<SimpleMessageEventArgs<MessageProcessingEventArgs>> MessageProcessingEnded;

        public AmqpClient(IFeedConfiguration config,
            IApiClient apiClient,
            EventHandler<CallbackExceptionEventArgs> onCallbackException,
            EventHandler<ShutdownEventArgs> onConnectionShutdown,
            IExchangeNameProvider exchangeNameProvider)
        {
            _host = config.Host;
            _port = config.Port;
            _username = config.AccessToken;
            _exceptionHandlingStrategy = config.ExceptionHandlingStrategy;
            _apiClient = apiClient;
            _onCallbackException = onCallbackException;
            _onConnectionShutdown = onConnectionShutdown;
            _exchangeNameProvider = exchangeNameProvider;
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            var bookmakerDetails = _apiClient.GetBookmakerDetails();

            var factory = new ConnectionFactory()
            {
                HostName = _host,
                Port = _port,
                UserName = _username,
                Password = "", // should be left blank
                VirtualHost = bookmakerDetails.VirtualHost,

                AutomaticRecoveryEnabled = true
            };

            factory.Ssl.Enabled = true;
            factory.Ssl.AcceptablePolicyErrors =
                // INFO: following acceptable errors make it so it's not necessary to install the server certificate
                System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch
                | System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable
                | System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors;

            return factory;
        }

        private void CreateConnectionFromConnectionFactory(ConnectionFactory factory)
        {
            try
            {
                _connection = factory.CreateConnection();
            }
            catch (BrokerUnreachableException)
            {
                var message = "AMQP connection factory was unable to create a connection to broker!";
                _log.LogWarning(message);
                throw new CommunicationException(message);
            }
        }

        public void Connect(MessageInterest messageInterest, IEnumerable<string> routingKeys)
        {
            _log.LogInformation($"Connecting {nameof(AmqpClient)} with message interest {messageInterest.Name}...");

            try
            {
                var factory = CreateConnectionFactory();

                CreateConnectionFromConnectionFactory(factory);
                _connection.CallbackException += _onCallbackException;
                _connection.ConnectionShutdown += _onConnectionShutdown;

                _channel = _connection.CreateModel();

                var queueInfo = _channel.QueueDeclare(
                    queue: "", // should be left blank
                    durable: false,
                    exclusive: true);

                _channel.ExchangeDeclare(
                    exchange: _exchangeNameProvider.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true);

                foreach (var routingKey in routingKeys)
                    _channel.QueueBind(queueInfo.QueueName, _exchangeNameProvider.ExchangeName, routingKey);

                _consumer = new EventingBasicConsumer(_channel);
                _consumer.Received += OnReceived;
                _channel.BasicConsume(queueInfo.QueueName, autoAck: true, _consumer);
            }
            catch (CommunicationException)
            {
                throw;
            }
            catch (Exception)
            {
                var message = "An exception was thrown when connecting to AMQP feed!";
                _log.LogError(message);
                throw new CommunicationException(message);
            }
        }

        private void HandleUnparsableMessage(byte[] messageBody, string messageRoutingKey)
        {
            MessageType messageType = MessageType.UNKNOWN;
            string producer = string.Empty;
            string eventId = string.Empty;
            try
            {
                messageType = TopicParsingHelper.GetMessageType(messageRoutingKey);
                producer = TopicParsingHelper.GetProducer(messageRoutingKey);
                eventId = TopicParsingHelper.GetEventId(messageRoutingKey);
            }
            catch (ArgumentException)
            {
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    throw;
            }
            Dispatch(UnparsableMessageReceived, new UnparsableMessageEventArgs(messageType, producer, eventId, messageBody), nameof(UnparsableMessageReceived));
        }

        private bool TryGetMessageSentTime(BasicDeliverEventArgs eventArgs, out long sentTime)
        {
            sentTime = 0;

            var headers = eventArgs?.BasicProperties?.Headers;
            if (headers is null)
                return false;

            var sentAtHeader = "timestamp_in_ms";
            if (headers.ContainsKey(sentAtHeader) == false)
                return false;

            var value = headers[sentAtHeader].ToString();
            if (long.TryParse(value, out var parseResult) == false)
                return false;

            sentTime = parseResult;
            return true;
        }

        private void SetMessageTimes(FeedMessageModel message, BasicDeliverEventArgs eventArgs, long receivedAt)
        {
            message.ReceivedAt = receivedAt;

            if (TryGetMessageSentTime(eventArgs, out var sentTime) == false)
            {
                //_log.LogInformation($"{GetType()} was unable to get message SentAt time. Setting SentAt to GeneratedAt ({DateTimeOffset.FromUnixTimeMilliseconds(message.GeneratedAt)}");
                message.SentAt = message.GeneratedAt;
            }
            else
            {
                message.SentAt = sentTime;
            }
        }

        private void OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var receivedAt = Timestamp.Now();
            var body = eventArgs.Body.ToArray();
            var xml = Encoding.UTF8.GetString(body);
            var success = FeedMessageDeserializer.TryDeserializeMessage(xml, out var message);

            if (success == false)
            {
                HandleUnparsableMessage(body, eventArgs.RoutingKey);
                return;
            }

            var producerId = message.ProducerId;

            Dispatch(MessageProcessingStarted,
                new SimpleMessageEventArgs<MessageProcessingEventArgs>(new MessageProcessingEventArgs(producerId, receivedAt), body),
                nameof(MessageProcessingStarted)
            );

            SetMessageTimes(message, eventArgs, receivedAt);

            long? timestamp = null;

            switch (message)
            {
                case alive aliveMessage:
                    var aliveMessageArgs = new SimpleMessageEventArgs<alive>(aliveMessage, body);
                    timestamp = aliveMessageArgs.FeedMessage.GeneratedAt;
                    Dispatch(AliveMessageReceived, aliveMessageArgs, nameof(AliveMessageReceived));
                    break;
                case snapshot_complete snapshotComplete:
                    var snapshotCompleteMessageArgs = new SimpleMessageEventArgs<snapshot_complete>(snapshotComplete, body);
                    Dispatch(SnapshotCompleteMessageReceived, snapshotCompleteMessageArgs, nameof(SnapshotCompleteMessageReceived));
                    break;
                case odds_change oddsChangeMessage:
                    var oddsChangeMessageArgs = new SimpleMessageEventArgs<odds_change>(oddsChangeMessage, body);
                    timestamp = oddsChangeMessageArgs.FeedMessage.GeneratedAt;
                    Dispatch(OddsChangeMessageReceived, oddsChangeMessageArgs, nameof(OddsChangeMessageReceived));
                    break;
                case bet_stop betStopMessage:
                    var betStopMessageArgs = new SimpleMessageEventArgs<bet_stop>(betStopMessage, body);
                    timestamp = betStopMessageArgs.FeedMessage.GeneratedAt;
                    Dispatch(BetStopMessageReceived, betStopMessageArgs, nameof(BetStopMessageReceived));
                    break;
                case bet_settlement betSettlement:
                    var betSettlementMessageArgs = new SimpleMessageEventArgs<bet_settlement>(betSettlement, body);
                    Dispatch(BetSettlementMessageReceived, betSettlementMessageArgs, nameof(BetSettlementMessageReceived));
                    break;
                case bet_cancel betCancel:
                    var betCancelMessageArgs = new SimpleMessageEventArgs<bet_cancel>(betCancel, body);
                    Dispatch(BetCancelMessageReceived, betCancelMessageArgs, nameof(BetCancelMessageReceived));
                    break;
                case fixture_change fixtureChange:
                    var fixtureChangeMessageArgs = new SimpleMessageEventArgs<fixture_change>(fixtureChange, body);
                    Dispatch(FixtureChangeMessageReceived, fixtureChangeMessageArgs, nameof(FixtureChangeMessageReceived));
                    break;

                default:
                    var errorMessage = $"FeedMessage of type '{message.GetType().Name}' is not supported.";
                    _log.LogError(errorMessage);
                    throw new InvalidOperationException(errorMessage);
            }

            Dispatch(MessageProcessingEnded,
                new SimpleMessageEventArgs<MessageProcessingEventArgs>(new MessageProcessingEventArgs(producerId, timestamp), body),
                nameof(MessageProcessingEnded)
                );
        }

        public void Disconnect()
        {
            _log.LogInformation($"Disconnecting {typeof(AmqpClient).Name}...");

            _consumer.Received -= OnReceived;
            _channel.Close();
            _connection.CallbackException -= _onCallbackException;
            _connection.ConnectionShutdown -= _onConnectionShutdown;
            try
            {
                _connection.Close();
            }
            catch (IOException)
            {
                _log.LogWarning("Socket was closed unexpectedly when closing AMQP connection!");
            }
            finally
            {
                _channel.Dispose();
                _connection.Dispose();
            }
        }
    }
}