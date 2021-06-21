using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Oddin.Oddin.SDK.API;
using Oddin.Oddin.SDK.Managers;
using Oddin.OddinSdk.SDK;
using Oddin.OddinSdk.SDK.AMQP;
using System;
using Unity;
using Unity.Injection;

namespace Oddin.Oddin.SDK
{
    public class Feed
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
            _unityContainer.RegisterType<IApiClient, ApiClient>(
                new InjectionConstructor(
                    _oddsFeedConfiguration,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
            _unityContainer.RegisterType<IAmqpClient, AmqpClient>(
                new InjectionConstructor(
                    _oddsFeedConfiguration,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
            _unityContainer.RegisterType<IProducerManager, ProducerManager>(
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


            // TODO: remove when amqp tested
            TestAmqp();
        }

        private void TestAmqp()
        {
            var rabbit = _unityContainer.Resolve<IAmqpClient>();
            rabbit.Connect();
        }
    }
}
