using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.API.Models;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
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

        public ApiClient(IFeedConfiguration config, ILoggerFactory loggerFactory)
        {
            _restClient = new RestClient(config, loggerFactory);
            _defaultCulture = config.DefaultLocale;
        }

        public List<IProducer> GetProducers()
        {
            var response = _restClient.SendRequest<ProducersModel>("v1/descriptions/producers", HttpMethod.Get);
            return ApiModelMapper.MapProducersList(response);
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
            return ApiModelMapper.MapMatchSummary(matchSummaryModel);
        }

        public async Task<List<IMarketDescription>> GetMarketDescriptionsAsync(CultureInfo desiredCulture = null)
        {
            var culture = desiredCulture is null ? _defaultCulture : desiredCulture;
            var route = $"v1/descriptions/{culture.TwoLetterISOLanguageName}/markets";
            var marketDescriptionsModel = await _restClient.SendRequestAsync<MarketDescriptionsModel>(route, HttpMethod.Get);
            return ApiModelMapper.MapMarketDescriptionsList(marketDescriptionsModel);
        }
    }
}
