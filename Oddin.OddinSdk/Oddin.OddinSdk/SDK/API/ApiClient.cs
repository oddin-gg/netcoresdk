using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System.Collections.Generic;
using System.Net.Http;

namespace Oddin.OddinSdk.SDK.API
{
    internal class ApiClient : IApiClient
    {
        private readonly RestClient _restClient;

        public ApiClient(IOddsFeedConfiguration config, ILoggerFactory loggerFactory)
        {
            _restClient = new RestClient(config, loggerFactory);
        }

        public List<IProducer> GetProducers()
        {
            var response = _restClient.SendRequest<ProducersModel>("v1/descriptions/producers", HttpMethod.Get);
            var result = new List<IProducer>();
            foreach (var producer in response.producer)
                result.Add(new Producer(producer));
            return result;
        }

        public IBookmakerDetails GetBookmakerDetails()
        {
            var response = _restClient.SendRequest<BookmakerDetailsModel>("v1/users/whoami", HttpMethod.Get);
            return new BookmakerDetails(response);
        }
    }
}
