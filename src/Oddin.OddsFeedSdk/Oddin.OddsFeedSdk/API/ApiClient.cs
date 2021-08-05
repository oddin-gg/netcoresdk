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
using System.Reactive.Subjects;

namespace Oddin.OddsFeedSdk.API
{
    internal class ApiClient : IApiClient
    {
        private readonly IApiModelMapper _apiModelMapper;
        private readonly IRestClient _restClient;
        private readonly CultureInfo _defaultCulture;
        private readonly Subject<object> _publisher = new Subject<object>();

        public ApiClient(IApiModelMapper apiModelMapper, IFeedConfiguration config, IRestClient restClient)
        {
            if (apiModelMapper is null)
                throw new ArgumentNullException(nameof(apiModelMapper));

            _apiModelMapper = apiModelMapper;
            _restClient = restClient;
            _defaultCulture = config.DefaultLocale;
        }

        public fixtureChangesEndpoint GetFixtureChanges(CultureInfo culture)
        {
            if (culture is null)
                culture = _defaultCulture;

            var route = $"v1/sports/${culture.TwoLetterISOLanguageName}/fixtures/changes";
            var result = _restClient.SendRequest<fixtureChangesEndpoint>(route, HttpMethod.Get);
            return result.Data;
        }

        public ScheduleEndpointModel GetSchedule(int startIndex, int limit, CultureInfo culture)
        {
            if (culture is null)
                culture = _defaultCulture;

            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/schedules/pre/schedule?start={startIndex}&limit={limit}";
            var result = _restClient.SendRequest<ScheduleEndpointModel>(route, HttpMethod.Get);
            return result.Data;
        }

        public ScheduleEndpointModel GetLiveMatches(CultureInfo culture)
        {
            if (culture is null)
                culture = _defaultCulture;

            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/schedules/live/schedule";
            var result = _restClient.SendRequest<ScheduleEndpointModel>(route, HttpMethod.Get);
            return result.Data;
        }

        public ScheduleEndpointModel GetMatches(DateTime dateToGet, CultureInfo culture)
        {
            if (culture is null)
                culture = _defaultCulture;

            var dateRoute = dateToGet.ToUniversalTime().ToString("yyyy-MM-dd");
            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/schedules/{dateRoute}/schedule";
            var result = _restClient.SendRequest<ScheduleEndpointModel>(route, HttpMethod.Get);
            return result.Data;
        }

        public MatchStatusModel GetMatchStatusDescriptions(CultureInfo culture)
        {
            if (culture is null)
                culture = _defaultCulture;

            var route = $"v1/descriptions/{culture.TwoLetterISOLanguageName}/match_status";
            var result = _restClient.SendRequest<MatchStatusModel>(route, HttpMethod.Get);
            return result.Data;
        }

        public FixturesEndpointModel GetFixture(URN id, CultureInfo culture)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (culture is null)
                culture = _defaultCulture;

            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/sport_events/{id}/fixture";
            var result = _restClient.SendRequest<FixturesEndpointModel>(route, HttpMethod.Get);
            return result.Data;
        }

        public teamExtended GetCompetitorProfile(URN id, CultureInfo culture)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (culture is null)
                culture = _defaultCulture;

            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/competitors/{id}/profile";
            var result = _restClient.SendRequest<competitorProfileEndpoint>(route, HttpMethod.Get, culture);
            return result.Data.competitor;

        }

        public TournamentInfoModel GetTournament(URN id, CultureInfo culture = null)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (culture is null)
                culture = _defaultCulture;
            
            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/tournaments/{id}/info";
            var result = _restClient.SendRequest<TournamentInfoModel>(route, HttpMethod.Get, culture);
            return result.Data;
        }
        
        public TournamentsModel GetTournaments(URN sportId, CultureInfo culture = null)
        {
            if (sportId is null)
                throw new ArgumentNullException(nameof(sportId));

            if (culture is null)
                culture = _defaultCulture;
            
            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/sports/{sportId}/tournaments";
            var result = _restClient.SendRequest<TournamentsModel>(route, HttpMethod.Get, culture);
            return result.Data;
        }

        public async Task<SportsModel> GetSports(CultureInfo culture)
        {
            if (culture is null)
                culture = _defaultCulture;

            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/sports";
            var result = await _restClient.SendRequestAsync<SportsModel>(route, HttpMethod.Get, culture);
            return result.Data;
        }

        public IEnumerable<IProducer> GetProducers()
        {
            var route = "v1/descriptions/producers";
            var response = _restClient.SendRequest<ProducersModel>(route, HttpMethod.Get);
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

            var response = await _restClient.SendRequestAsync<MatchSummaryModel>(route, HttpMethod.Get, culture);
            return _apiModelMapper.MapMatchSummary(response.Data);
        }

        public MatchSummaryModel GetMatchSummary(URN sportEventId, CultureInfo desiredCulture)
        {
            var culture = desiredCulture ?? _defaultCulture;
            var route = $"v1/sports/{culture.TwoLetterISOLanguageName}/sport_events/{sportEventId}/summary";

            var response = _restClient.SendRequest<MatchSummaryModel>(route, HttpMethod.Get, culture);
            return response.Data;
        }

        public async Task<MarketDescriptionsModel> GetMarketDescriptionsAsync(CultureInfo desiredCulture = null)
        {
            var culture = desiredCulture is null ? _defaultCulture : desiredCulture;
            var route = $"v1/descriptions/{culture.TwoLetterISOLanguageName}/markets";
            var response = await _restClient.SendRequestAsync<MarketDescriptionsModel>(route, HttpMethod.Get);
            return response.Data;
        }

        public async Task<long> PostEventRecoveryRequest(string producerName, URN sportEventId, long requestId, int? nodeId)
        {
            var route = $"v1/{producerName}/odds/events/{sportEventId}/initiate_request";
            var parameters = new List<(string key, object value)>
            {
                ("request_id", requestId)
            };

            if (nodeId.HasValue)
                parameters.Add(("node_id", nodeId));

            var response = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters.ToArray(), deserializeResponse: false, ignoreUnsuccessfulStatusCode: true);
            return (long)response.ResponseCode;
        }

        public async Task<long> PostEventStatefulRecoveryRequest(string producerName, URN sportEventId, long requestId, int? nodeId)
        {
            var route = $"v1/{producerName}/stateful_messages/events/{sportEventId}/initiate_request";
            var parameters = new List<(string key, object value)>
            {
                ("request_id", requestId)
            };

            if (nodeId.HasValue)
                parameters.Add(("node_id", nodeId));

            var response = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters.ToArray(), deserializeResponse: false, ignoreUnsuccessfulStatusCode: true);
            return (long)response.ResponseCode;
        }

        public async Task PostRecoveryRequest(string producerName, long requestId, int? nodeId, DateTime timestamp = default)
        {
            var route = $"v1/{producerName}/recovery/initiate_request";
            List<(string key, object value)> parameters;

            parameters = new List<(string key, object value)>
            {
                ("request_id", requestId)
            };

            if (timestamp != default)
                parameters.Add(("after", timestamp.ToEpochTimeMilliseconds()));

            if (nodeId != null)
                parameters.Add(("node_id", nodeId));

            await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters.ToArray(), deserializeResponse: false);
        }

        public IObservable<T> SubscribeForClass<T>() => _restClient.SubscribeForClass<T>();

        public async Task<bool> PostReplayClear(int? nodeId)
        {
            var route = $"v1/replay/clear";
            var parameters = ParametersOrDefault(nodeId);

            var result = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters, deserializeResponse: false);
            return result.Successful;
        }

        public async Task<bool> PostReplayStop(int? nodeId)
        {
            var route = $"v1/replay/stop";
            var parameters = ParametersOrDefault(nodeId);

            var result = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters, deserializeResponse: false);
            return result.Successful;
        }

        public async Task<ReplayEndpointModel> GetReplaySetContent(int? nodeId)
        {
            var route = $"v1/replay";
            var parameters = ParametersOrDefault(nodeId);

            var result = await _restClient.SendRequestAsync<ReplayEndpointModel>(route, HttpMethod.Get, parameters: parameters);
            return result.Data;
        }

        public async Task<bool> PutReplayEvent(URN eventId, int? nodeId)
        {
            var route = $"v1/replay/events/{eventId}";
            var parameters = ParametersOrDefault(nodeId);

            var result = await _restClient.SendRequestAsync<object>(route, HttpMethod.Put, parameters: parameters, deserializeResponse: false);
            return result.Successful;
        }

        public async Task<bool> DeleteReplayEvent(URN eventId, int? nodeId)
        {
            var route = $"v1/replay/events/{eventId}";
            var parameters = ParametersOrDefault(nodeId);

            var result = await _restClient.SendRequestAsync<object>(route, HttpMethod.Delete, parameters: parameters, deserializeResponse: false);
            return result.Successful;
        }

        public async Task<bool> PostReplayStart(int? nodeId, int? speed = null, int? maxDelay = null, bool? useReplayTimestamp = null, string product = null, bool? runParallel = null)
        {
            var parameters = new List<(string key, object value)>();
            if (nodeId != null)
                parameters.Add(("node_id", nodeId));

            if (speed != null)
                parameters.Add(("speed", speed));

            if (maxDelay != null)
                parameters.Add(("max_delay", maxDelay));

            if (useReplayTimestamp != null)
                parameters.Add(("use_replay_timestamp", useReplayTimestamp));

            if (runParallel != null)
                parameters.Add(("run_parallel", runParallel));

            if (product != null)
                parameters.Add(("product", product));

            var route = "/replay/play";

            var result = await _restClient.SendRequestAsync<object>(route, HttpMethod.Post, parameters: parameters.ToArray(), deserializeResponse: false);
            return result.Successful;
        }

        private (string key, object value)[] ParametersOrDefault(int? nodeId)
            => nodeId == null
                ? default
                : new (string key, object value)[] { ("node_id", nodeId.Value) };
    }
}
