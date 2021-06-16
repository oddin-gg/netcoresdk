using Oddin.Oddin.SDK.API;
using Oddin.Oddin.SDK.Managers;
using Unity;
using Unity.Injection;

namespace Oddin.Oddin.SDK
{
    public class Feed
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


        private void InitializeUnityContainer()
        {
            _unityContainer.RegisterType<IApiClient, ApiClient>();
            _unityContainer.RegisterType<IProducerManager, ProducerManager>(new InjectionConstructor(_unityContainer.Resolve<IApiClient>()));
        }

        public Feed()
        {
            _unityContainer = new UnityContainer();
            InitializeUnityContainer();
        }
    }
}
