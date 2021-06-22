using Microsoft.Extensions.Logging;
using Oddin.Oddin.SDK.Managers;
using Oddin.OddinSdk.Common.Exceptions;
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
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private IConnection _connection;
        private IModel _channel;

        public const string EXCHANGE_NAME = "oddinfeed";
        public const string ALL_MESSAGES_ROUTING_KEY = "#";

        public AmqpClient(IOddsFeedConfiguration config, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _host = config.Host;
            _port = config.Port;
            _username = config.AccessToken;
            _exceptionHandlingStrategy = config.ExceptionHandlingStrategy;
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _host,
                Port = _port,
                UserName = _username,
                Password = "", // should be left blank

                // TODO: fill in through ~ WhoAmIProvider
                VirtualHost = "/oddinfeed/1",

                AutomaticRecoveryEnabled = true
            };

            factory.Ssl.Enabled = true;
            factory.Ssl.AcceptablePolicyErrors =
                System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch

                // TODO: remove???
                // apparently not necessarily needed
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
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    throw new CommunicationException(message);
            }
        }

        public void Connect()
        {
            var factory = CreateConnectionFactory();

            // TODO: set OnCallbackException and OnConnectionShutdown (on factory)

            CreateConnectionFromConnectionFactory(factory);
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

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += Receive;
            _channel.BasicConsume(queueInfo.QueueName, autoAck: true, consumer);
        }

        // TODO: replace with event invokation
        private void Receive(object sender, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Rabbit received: {message}");
        }

        public void Disconnect()
        {
            _channel.Close();
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

    public interface IAmqpClient
    {
        /// <summary>
        /// Connects the AMQP consumer to the AMQP broker
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects the AMQP consumer from the AMQP broker
        /// </summary>
        void Disconnect();
    }
}
