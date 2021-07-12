using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.API;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.Managers;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.Configuration;
using System;
using Unity;
using Unity.Injection;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Configuration;
using Oddin.OddinSdk.SDK.Configuration;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;

namespace Oddin.OddinSdk.SDK
{
    public class Feed : DispatcherBase, IOddsFeed
    {
        private readonly IUnityContainer _unityContainer;

        /// <summary>
        /// Gets a <see cref="IProducerManager" /> instance used to retrieve producer related data
        /// </summary>
        /// <value>The producer manager</value>
        public IProducerManager ProducerManager
        {
            get => _unityContainer.Resolve<IProducerManager>();
        }
        
        /// <summary>
        /// Gets a <see cref="IBookmakerDetails"/> instance used to get info about bookmaker and token used
        /// </summary>
        public IBookmakerDetails BookmakerDetails
        {
            get => _unityContainer.Resolve<IApiClient>().GetBookmakerDetails();
        }


        private void RegisterObjectsToUnityContainer(IFeedConfiguration config, ILoggerFactory loggerFactory)
        {
            // INFO: registration order matters!

            // register existing logger factory
            _unityContainer.RegisterInstance(typeof(ILoggerFactory), loggerFactory);
            
            // register ApiClient as singleton
            _unityContainer.RegisterSingleton<IApiClient, ApiClient>(
                new InjectionConstructor(
                    config,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );

            // register FeedMessageDeserializer as singleton
            _unityContainer.RegisterSingleton<IFeedMessageDeserializer, FeedMessageDeserializer>(
                new InjectionConstructor());

            // register Amqp client as singleton
            _unityContainer.RegisterSingleton<IAmqpClient, AmqpClient>(
                new InjectionConstructor(
                    config,
                    BookmakerDetails.VirtualHost,
                    (EventHandler<CallbackExceptionEventArgs>)OnAmqpCallbackException,
                    (EventHandler<ShutdownEventArgs>)OnConnectionShutdown,
                    _unityContainer.Resolve<IFeedMessageDeserializer>(),
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
            
            // register ProducerManager as singleton
            _unityContainer.RegisterSingleton<IProducerManager, ProducerManager>(
                new InjectionConstructor(
                    _unityContainer.Resolve<IApiClient>(),
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Feed"/> class
        /// </summary>
        /// <param name="config">Feed configuration</param>
        /// <param name="loggerFactory">Logger factory</param>
        /// <exception cref="ArgumentNullException"/>
        public Feed(IFeedConfiguration config, ILoggerFactory loggerFactory = null) : base(loggerFactory)
        {
            if (config is null)
                throw new ArgumentNullException();

            _unityContainer = new UnityContainer();
            RegisterObjectsToUnityContainer(config, loggerFactory);
        }

        /// <summary>
        /// Constructs a <see cref="IFeedConfiguration"/> instance from provided information
        /// </summary>
        /// <returns>A <see cref="IFeedConfiguration"/> instance created from provided information</returns>
        /// <summary>
        /// Constructs a <see cref="IFeedConfiguration"/> instance from provided information
        /// </summary>
        /// <returns>A <see cref="IFeedConfiguration"/> instance created from provided information</returns>
        public static ITokenSetter GetConfigurationBuilder()
        {
            return new TokenSetter(new AppConfigurationSectionProvider());
        }
        
        /// <summary>
        /// Get all available languages that can be used within the SDK and are supported by the messages
        /// </summary>
        /// <returns>IEnumerable&lt;CultureInfo&gt;</returns>
        public static IEnumerable<CultureInfo> AvailableLanguages()
        {
            var codes = new[] { "en" };
            return codes
                .Select(c => CultureInfo.GetCultureInfo(c))
                .OrderBy(c => c.Name);
        }


        /// <summary>
        /// Opens connection to the feed
        /// </summary>
        /// <exception cref="CommunicationException"/>
        public void Open()
        {
            _unityContainer.Resolve<IAmqpClient>().Connect();
        }

        public void Close()
        {
            _unityContainer.Resolve<IAmqpClient>().Disconnect();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void OnAmqpCallbackException(object sender, CallbackExceptionEventArgs eventArgs)
        {
            Dispatch(ConnectionException, new ConnectionExceptionEventArgs(eventArgs.Exception, eventArgs.Detail), nameof(ConnectionException));
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            _log.LogWarning($"The AMQP connection was shut down. Cause: {shutdownEventArgs.Cause}");
            
            // TODO: handle feed recovery

            Dispatch(Disconnected, new EventArgs(), nameof(Disconnected));
        }

        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        public event EventHandler<ConnectionExceptionEventArgs> ConnectionException;

        /// <summary>
        /// Raised when the current instance of <see cref="IOddsFeed"/> loses connection to the feed
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;
    }

    public interface IOddsFeed : IDisposable
    {
        /// <summary>
        /// Gets a <see cref="IProducerManager"/> instance used to retrieve producer related data
        /// </summary>
        IProducerManager ProducerManager { get; }

        /// <summary>
        /// Opens the current feed by opening all created sessions
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the current feed by closing all created sessions and disposing of all resources associated with the current instance
        /// </summary>
        void Close();

        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
    }
}
