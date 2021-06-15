using NetGlade.Oddin.SDK.API;
using NetGlade.Oddin.SDK.API.Entities;
using System;
using System.Collections.Generic;

namespace NetGlade.Oddin.SDK.Managers.Internal
{
    internal class ProducerManager : IProducerManager
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


        public ProducerManager(ApiClient apiClient)
        {
            _producers = apiClient.GetProducers();
        }
    }
}
