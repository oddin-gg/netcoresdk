using Microsoft.Extensions.Logging;
using Oddin.Oddin.SDK.Managers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Oddin.OddinSdk.SDK.AMQP
{
    internal class AmqpClient : LoggingBase, IAmqpClient
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;

        public AmqpClient(IOddsFeedConfiguration config, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _host = config.Host;
            _port = config.Port;
            _username = config.AccessToken;
        }



        public void Connect()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _host,
                Port = _port,
                UserName = _username,
                Password = "",

                // TODO: fill in through ~ WhoAmIProvider
                VirtualHost = "/oddinfeed/1",

                AutomaticRecoveryEnabled = true
            };

            factory.Ssl.Enabled = true;
            factory.Ssl.AcceptablePolicyErrors =
                System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch

                // apparently not necessarily needed
                //| System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable
                //| System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
                ;

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queueInfo = channel.QueueDeclare(
                    queue: "",
                    durable: false,
                    exclusive: true);
                
                channel.ExchangeDeclare(
                    exchange: "oddinfeed",
                    type: ExchangeType.Topic,
                    durable: true);

                channel.QueueBind(queueInfo.QueueName, "oddinfeed", "#");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Receive;
                channel.BasicConsume(queueInfo.QueueName, autoAck: true, consumer);
            }
        }

        private void Receive(object sender, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Rabbit received: {message}");
        }
    }

    public interface IAmqpClient
    {
        /// <summary>
        /// Connects the AMQP  consumer to the AMQP broker
        /// </summary>
        void Connect();
    }
}
