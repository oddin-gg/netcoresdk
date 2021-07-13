using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.API
{
    internal class ApiClient : IApiClient
    {
        private readonly IApiModelMapper _apiModelMapper;
        private readonly RestClient _restClient;
        private readonly CultureInfo _defaultCulture;

        public ApiClient(IApiModelMapper apiModelMapper, IFeedConfiguration config, ILoggerFactory loggerFactory)
        {
            if (apiModelMapper is null)
                throw new ArgumentNullException(nameof(apiModelMapper));

            _apiModelMapper = apiModelMapper;
            _restClient = new RestClient(config, loggerFactory);
            _defaultCulture = config.DefaultLocale;
        }

        public List<IProducer> GetProducers()
        {
            var response = _restClient.SendRequest<ProducersModel>("v1/descriptions/producers", HttpMethod.Get);
            return _apiModelMapper.MapProducersList(response.Data);
        }

        public IBookmakerDetails GetBookmakerDetails()
        {
            var response = _restClient.SendRequest<BookmakerDetailsModel>("v1/users/whoami", HttpMethod.Get);
            return _apiModelMapper.MapBookmakerDetails(response.Data);
        }

        public async Task<IMatchSummary> GetMatchSummaryAsync(URN sportEventId, CultureInfo desiredCulture = null)
        {
            var culture = desiredCulture is null ? _defaultCulture : desiredCulture;
            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/sport_events/{sportEventId}/summary";
            var response = await _restClient.SendRequestAsync<MatchSummaryModel>(route, HttpMethod.Get);
            return _apiModelMapper.MapMatchSummary(response.Data);
        }

        public async Task<List<IMarketDescription>> GetMarketDescriptionsAsync(CultureInfo desiredCulture = null)
        {
            var culture = desiredCulture is null ? _defaultCulture : desiredCulture;
            var route = $"v1/descriptions/{culture.TwoLetterISOLanguageName}/markets";
            var response = await _restClient.SendRequestAsync<MarketDescriptionsModel>(route, HttpMethod.Get);
            return _apiModelMapper.MapMarketDescriptionsList(response.Data);
        }

        public async Task<long> PostEventRecoveryRequest(string producerName, URN sportEventId)
        {
            var route = $"v1/{producerName}/odds/events/{sportEventId}/initiate_request";
            var response = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, deserializeResponse: false);
            return (long)response.ResponseCode;
        }

        public async Task<long> PostEventStatefulRecoveryRequest(string producerName, URN sportEventId)
        {
            var route = $"v1/{producerName}/stateful_messages/events/{sportEventId}/initiate_request";
            var response = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, deserializeResponse: false);
            return (long)response.ResponseCode;
        }
    }
}
