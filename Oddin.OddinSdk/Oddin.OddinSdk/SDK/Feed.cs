﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Oddin.OddinSdk.SDK.API;
using Oddin.OddinSdk.SDK.Managers;
using Oddin.OddinSdk.SDK;
using System;
using Unity;
using Unity.Injection;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;

namespace Oddin.OddinSdk.SDK
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
            // register existing logger factory
            _unityContainer.RegisterInstance(typeof(ILoggerFactory), _loggerFactory);

            // register ApiClient as singleton
            _unityContainer.RegisterType<IApiClient, ApiClient>(
                new InjectionConstructor(
                    _oddsFeedConfiguration,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
            
            // register ProducerManager as singleton
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
        }
    }
}