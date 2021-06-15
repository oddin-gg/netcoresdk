using NetGlade.Oddin.SDK.API.Entities;
using NetGlade.Oddin.SDK.API.Entities.Internal;
using System.Collections.Generic;

namespace NetGlade.Oddin.SDK.API
{
    internal class ApiClient : IApiClient
    {
        public List<IProducer> GetProducers()
            => new List<IProducer>()
            {
                new Producer(name: "producer's name"),
            };
    }

    public interface IApiClient
    {
        List<IProducer> GetProducers();
    }
}
