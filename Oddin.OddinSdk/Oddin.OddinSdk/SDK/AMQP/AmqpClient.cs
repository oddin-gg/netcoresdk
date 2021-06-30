using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common.Exceptions;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Text;

namespace Oddin.OddinSdk.SDK.AMQP
{
    internal class AmqpClient : LoggingBase, IAmqpClient
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _virtualHost;
        private readonly EventHandler<CallbackExceptionEventArgs> _onCallbackException;
        private readonly EventHandler<ShutdownEventArgs> _onConnectionShutdown;
        private readonly FeedMessageDeserializer _deserializer;
        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;

        public const string EXCHANGE_NAME = "oddinfeed";
        public const string ALL_MESSAGES_ROUTING_KEY = "#";
        public const string ALIVE_MESSAGES_ROUTING_KEY = "-.-.-.alive.-.-.-.-";

        public AmqpClient(IOddsFeedConfiguration config,
            string virtualHost,
            EventHandler<CallbackExceptionEventArgs> onCallbackException,
            EventHandler<ShutdownEventArgs> onConnectionShutdown,
            FeedMessageDeserializer deserializer,
            ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _host = config.Host;
            _port = config.Port;
            _username = config.AccessToken;
            _virtualHost = virtualHost;
            _onCallbackException = onCallbackException;
            _onConnectionShutdown = onConnectionShutdown;
            _deserializer = deserializer;
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _host,
                Port = _port,
                UserName = _username,
                Password = "", // should be left blank
                VirtualHost = _virtualHost,

                AutomaticRecoveryEnabled = true
            };

            factory.Ssl.Enabled = true;
            factory.Ssl.AcceptablePolicyErrors =
                // INFO: following acceptable error makes it so it's not necessary to install the server certificate
                System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch

                // TODO: remove the following ones? - apparently not necessarily needed (UO uses them)
                //| System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable
                //| System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
                ;

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

        public void Connect()
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
                exchange: EXCHANGE_NAME,
                type: ExchangeType.Topic,
                durable: true);

            _channel.QueueBind(queueInfo.QueueName, EXCHANGE_NAME, ALL_MESSAGES_ROUTING_KEY);

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += OnReceived;
            _channel.BasicConsume(queueInfo.QueueName, autoAck: true, _consumer);
        }

        private void OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body.ToArray();
            var xml = Encoding.UTF8.GetString(body);
            var success = _deserializer.TryDeserializeMessage(xml, out var message);

            if (success == false)
            {
                var messageType = TopicParsingHelper.GetMessageType(eventArgs.RoutingKey);
                UnparsableMessageReceived(this, new UnparsableMessageEventArgs(messageType, body));
            }

            switch (message)
            {
                case AliveMessage aliveMessage:
                    AliveMessageReceived(this, new MessageEventArgs<AliveMessage>(aliveMessage, body));
                    break;
                case OddsChangeMessage oddsChangeMessage:
                    OddsChangeMessageReceived(this, new MessageEventArgs<OddsChangeMessage>(oddsChangeMessage, body));
                    break;

                    // ...

                default:
                    var errorMessage = $"FeedMessage of type '{message.GetType().Name}' is not supported.";
                    _log.LogError(errorMessage);
                    throw new InvalidOperationException(errorMessage);
            }
        }

        public void Disconnect()
        {
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

        public event EventHandler<UnparsableMessageEventArgs> UnparsableMessageReceived;

        public event EventHandler<MessageEventArgs<AliveMessage>> AliveMessageReceived;

        public event EventHandler<MessageEventArgs<OddsChangeMessage>> OddsChangeMessageReceived;
    }
}
