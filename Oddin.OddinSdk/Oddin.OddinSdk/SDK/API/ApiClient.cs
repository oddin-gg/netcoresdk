using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.API
{
    internal class ApiClient : IApiClient
    {
        private readonly RestClient _restClient;
        private readonly CultureInfo _defaultCulture;

        public ApiClient(IOddsFeedConfiguration config, ILoggerFactory loggerFactory)
        {
            _restClient = new RestClient(config, loggerFactory);
            _defaultCulture = config.DefaultLocale;
        }

        public List<IProducer> GetProducers()
        {
            var response = _restClient.SendRequest<ProducersModel>("v1/descriptions/producers", HttpMethod.Get);
            
            var result = new List<IProducer>();
            if (response.producer is null)
                return result;
            foreach (var producer in response.producer)
                result.Add(new Producer(producer));
            return result;
        }

        public IBookmakerDetails GetBookmakerDetails()
        {
            var response = _restClient.SendRequest<BookmakerDetailsModel>("v1/users/whoami", HttpMethod.Get);
            return ApiModelMapper.MapBookmakerDetails(response);
        }

        public async Task<IMatchSummary> GetMatchSummaryAsync(URN sportEventId, CultureInfo desiredCulture = null)
        {
            var culture = desiredCulture is null ? _defaultCulture : desiredCulture;
            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/sport_events/{sportEventId.Urn}/summary";
            var matchSummaryModel = await _restClient.SendRequestAsync<MatchSummaryModel>(route, HttpMethod.Get);
            return new MatchSummary(matchSummaryModel);
        }

        public async Task<List<IMarketDescription>> GetMarketDescriptionsAsync(CultureInfo desiredCulture = null)
        {
            var culture = desiredCulture is null ? _defaultCulture : desiredCulture;
            var route = $"v1/descriptions/{culture.TwoLetterISOLanguageName}/markets";
            var marketDescriptionsModel = await _restClient.SendRequestAsync<MarketDescriptionsModel>(route, HttpMethod.Get);
            
            var result = new List<IMarketDescription>();
            if (marketDescriptionsModel.market is null)
                return result;
            foreach (var marketDescription in marketDescriptionsModel.market)
                result.Add(ApiModelMapper.MapMarketDescription(marketDescription));
            return result;
        }
    }
}
