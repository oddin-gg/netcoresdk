using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Oddin.Oddin.SDK.API;
using Oddin.Oddin.SDK.Managers;
using Oddin.OddinSdk.SDK;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System;
using Unity;
using Unity.Injection;

namespace Oddin.Oddin.SDK
{
    public class Feed : IOddsFeed
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnityContainer _unityContainer;
        private readonly IOddsFeedConfiguration _oddsFeedConfiguration;

        /// <summary>
        /// Gets a <see cref="IProducerManager" /> instance used to retrieve producer related data
        /// </summary>
        /// <value>The producer manager</value>
        public IProducerManager ProducerManager
        {
            get => _unityContainer.Resolve<IProducerManager>();
        }


        private void RegisterObjectsToUnityContainer()
        {
            _unityContainer.RegisterInstance(typeof(ILoggerFactory), _loggerFactory);
            _unityContainer.RegisterSingleton<IApiClient, ApiClient>(
                new InjectionConstructor(
                    _oddsFeedConfiguration,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
            _unityContainer.RegisterSingleton<IAmqpClient, AmqpClient>(
                new InjectionConstructor(
                    _oddsFeedConfiguration,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
            _unityContainer.RegisterSingleton<IProducerManager, ProducerManager>(
                new InjectionConstructor(
                    _unityContainer.Resolve<IApiClient>(),
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
        }

        public Feed(IOddsFeedConfiguration config, ILoggerFactory loggerFactory = null)
        {
            if (config is null)
                throw new ArgumentNullException();
            _oddsFeedConfiguration = config;

            _loggerFactory = loggerFactory ?? new NullLoggerFactory();

            _unityContainer = new UnityContainer();
            RegisterObjectsToUnityContainer();
        }

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

        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        public event EventHandler<ConnectionExceptionEventArgs> ConnectionException;


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
