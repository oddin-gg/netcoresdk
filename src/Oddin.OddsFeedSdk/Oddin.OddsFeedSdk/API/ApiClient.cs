using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal class ApiClient : IApiClient
    {
        private readonly IApiModelMapper _apiModelMapper;
        private readonly RestClient _restClient;
        private readonly CultureInfo _defaultCulture;

        public ApiClient(IApiModelMapper apiModelMapper, IFeedConfiguration config)
        {
            if (apiModelMapper is null)
                throw new ArgumentNullException(nameof(apiModelMapper));

            _apiModelMapper = apiModelMapper;
            _restClient = new RestClient(config);
            _defaultCulture = config.DefaultLocale;
        }

        public IEnumerable<IProducer> GetProducers()
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

        public async Task<IEnumerable<IMarketDescription>> GetMarketDescriptionsAsync(CultureInfo desiredCulture = null)
        {
            var culture = desiredCulture is null ? _defaultCulture : desiredCulture;
            var route = $"v1/descriptions/{culture.TwoLetterISOLanguageName}/markets";
            var response = await _restClient.SendRequestAsync<MarketDescriptionsModel>(route, HttpMethod.Get);
            return _apiModelMapper.MapMarketDescriptionsList(response.Data);
        }

        public async Task<long> PostEventRecoveryRequest(string producerName, URN sportEventId, long requestId)
        {
            var route = $"v1/{producerName}/odds/events/{sportEventId}/initiate_request";
            var parameters = new (string key, object value)[]
            {
                ("request_id", requestId)
            };

            var response = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters, deserializeResponse: false, ignoreUnsuccessfulStatusCode: true);
            return (long)response.ResponseCode;
        }

        public async Task<long> PostEventStatefulRecoveryRequest(string producerName, URN sportEventId, long requestId)
        {
            var route = $"v1/{producerName}/stateful_messages/events/{sportEventId}/initiate_request";
            var parameters = new (string key, object value)[]
            {
                ("request_id", requestId)
            };

            var response = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters, deserializeResponse: false, ignoreUnsuccessfulStatusCode: true);
            return (long)response.ResponseCode;
        }

        public async Task PostRecoveryRequest(string producerName, long requestId, int nodeId, DateTime timestamp = default)
        {
            var route = $"v1/{producerName}/recovery/initiate_request";
            (string key, object value)[] parameters;

            if (timestamp == default)
                parameters = new (string key, object value)[]
                {
                    ("request_id", requestId),
                    ("node_id", nodeId)
                };
            else
                parameters = new (string key, object value)[]
                {
                    ("after", timestamp.ToEpochTimeMilliseconds()),
                    ("request_id", requestId),
                    ("node_id", nodeId)
                };

            await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters, deserializeResponse: false);
        }
    }
}
