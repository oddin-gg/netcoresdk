using Oddin.Oddin.SDK.API.Entities;
using Oddin.Oddin.SDK.API.Entities.Internal;
using System.Collections.Generic;

namespace Oddin.Oddin.SDK.API
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
