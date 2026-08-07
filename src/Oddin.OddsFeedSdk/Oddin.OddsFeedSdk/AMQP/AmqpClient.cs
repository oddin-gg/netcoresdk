using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Sessions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Oddin.OddsFeedSdk.AMQP;

internal class AmqpClient : DispatcherBase, IAmqpClient
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(AmqpClient));
    private readonly IApiClient _apiClient;
    private readonly IExchangeNameProvider _exchangeNameProvider;

    private readonly string _host;
    private readonly AsyncEventHandler<CallbackExceptionEventArgs> _onCallbackException;
    private readonly AsyncEventHandler<ShutdownEventArgs> _onConnectionShutdown;
    private readonly int _port;
    private readonly string _username;
    private IChannel _channel;
    private IConnection _connection;
    private AsyncEventingBasicConsumer _consumer;


    public AmqpClient(IFeedConfiguration config,
        IApiClient apiClient,
        AsyncEventHandler<CallbackExceptionEventArgs> onCallbackException,
        AsyncEventHandler<ShutdownEventArgs> onConnectionShutdown,
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

    public event AsyncEventHandler<BasicDeliverEventArgs> OnReceived;

    public async Task Connect(MessageInterest messageInterest, IEnumerable<string> routingKeys)
    {
        _log.LogInformation($"Connecting {nameof(AmqpClient)} with message interest {messageInterest.Name}...");

        try
        {
            var factory = CreateConnectionFactory();

            await CreateConnectionFromConnectionFactory(factory);
            _connection.CallbackExceptionAsync += _onCallbackException;
            _connection.ConnectionShutdownAsync += _onConnectionShutdown;

            _channel = await _connection.CreateChannelAsync();

            var queueInfo = await _channel.QueueDeclareAsync();

            await _channel.ExchangeDeclareAsync(
                _exchangeNameProvider.ExchangeName,
                ExchangeType.Topic,
                true);

            foreach (var routingKey in routingKeys)
                await _channel.QueueBindAsync(queueInfo.QueueName, _exchangeNameProvider.ExchangeName, routingKey);

            _consumer = new AsyncEventingBasicConsumer(_channel);
            _consumer.ReceivedAsync += OnReceived;
            await _channel.BasicConsumeAsync(queueInfo.QueueName, true, _consumer);
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


    public async Task Disconnect()
    {
        _log.LogInformation($"Disconnecting {nameof(AmqpClient)}...");

        _consumer.ReceivedAsync -= OnReceived;
        await _channel.CloseAsync();
        _connection.CallbackExceptionAsync -= _onCallbackException;
        _connection.ConnectionShutdownAsync -= _onConnectionShutdown;
        try
        {
            await _connection.CloseAsync();
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
                AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch
                                         | SslPolicyErrors.RemoteCertificateNotAvailable
                                         | SslPolicyErrors.RemoteCertificateChainErrors
            }
        };

        return factory;
    }

    private async Task CreateConnectionFromConnectionFactory(ConnectionFactory factory)
    {
        try
        {
            _connection = await factory.CreateConnectionAsync();
        }
        catch (BrokerUnreachableException)
        {
            var message = "AMQP connection factory was unable to create a connection to broker!";
            _log.LogWarning(message);
            throw new CommunicationException(message);
        }
    }
}