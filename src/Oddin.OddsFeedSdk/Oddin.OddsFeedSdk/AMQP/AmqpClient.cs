using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Sessions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP
{
    internal class AmqpClient : DispatcherBase, IAmqpClient
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(AmqpClient));

        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly IApiClient _apiClient;
        private readonly EventHandler<CallbackExceptionEventArgs> _onCallbackException;
        private readonly EventHandler<ShutdownEventArgs> _onConnectionShutdown;
        private readonly IExchangeNameProvider _exchangeNameProvider;
        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;

        public event EventHandler<BasicDeliverEventArgs> OnReceived;


        public AmqpClient(IFeedConfiguration config,
            IApiClient apiClient,
            EventHandler<CallbackExceptionEventArgs> onCallbackException,
            EventHandler<ShutdownEventArgs> onConnectionShutdown,
            IExchangeNameProvider exchangeNameProvider)
        {
            _host = config.Host;
            _port = config.Port;
            _username = config.AccessToken;
            _apiClient = apiClient;
            _onCallbackException = onCallbackException;
            _onConnectionShutdown = onConnectionShutdown;
            _exchangeNameProvider = exchangeNameProvider;
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            var bookmakerDetails = _apiClient.GetBookmakerDetails();

            var factory = new ConnectionFactory
            {
                HostName = _host,
                Port = _port,
                UserName = _username,
                Password = "", // should be left blank
                VirtualHost = bookmakerDetails.VirtualHost,

                AutomaticRecoveryEnabled = true,
                Ssl =
                {
                    Enabled = true,
                    AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch
                                             | System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable
                                             | System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
                }
            };

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


        public void Disconnect()
        {
            _log.LogInformation($"Disconnecting {nameof(AmqpClient)}...");

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