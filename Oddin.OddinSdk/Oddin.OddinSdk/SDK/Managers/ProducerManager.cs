using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.API;
using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.Managers
{
    internal class ProducerManager : LoggingBase, IProducerManager
    {
        private IReadOnlyCollection<IProducer> _producers;


        public IReadOnlyCollection<IProducer> Producers => _producers;

        public void AddTimestampBeforeDisconnect(int id, DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public void DisableProducer(int id)
        {
            throw new NotImplementedException();
        }

        public bool Exists(int id)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string name)
        {
            throw new NotImplementedException();
        }

        public IProducer Get(int id)
        {
            throw new NotImplementedException();
        }

        public IProducer Get(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveTimestampBeforeDisconnect(int id)
        {
            throw new NotImplementedException();
        }


        public ProducerManager(ApiClient apiClient, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            // TODO: add cache
            // TODO: move to Producers getter
            _producers = apiClient.GetProducers();
        }
    }
}
