using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Oddin.Oddin.SDK.API;
using Oddin.Oddin.SDK.Managers;
using Unity;
using Unity.Injection;

namespace Oddin.Oddin.SDK
{
    public class Feed
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnityContainer _unityContainer;

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
            _unityContainer.RegisterType<IApiClient, ApiClient>();
            _unityContainer.RegisterType<IProducerManager, ProducerManager>(
                new InjectionConstructor(
                    _unityContainer.Resolve<IApiClient>(),
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
        }

        public Feed(ILoggerFactory loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? new NullLoggerFactory();

            _unityContainer = new UnityContainer();
            RegisterObjectsToUnityContainer();
        }
    }
}
