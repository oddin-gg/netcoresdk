using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    public interface IApiClient
    {
        IEnumerable<IProducer> GetProducers();

        IBookmakerDetails GetBookmakerDetails();

        Task<IMatchSummary> GetMatchSummaryAsync(URN sportEventId, CultureInfo culture = null);

        Task<SportsModel> GetSports(CultureInfo culture = null);

        TournamentsModel GetTournaments(URN sportId, CultureInfo culture = null);

        Task<MarketDescriptionsModel> GetMarketDescriptionsAsync(CultureInfo culture = null);

        Task<long> PostEventRecoveryRequest(string producerName, URN sportEventId, long requestId);

        Task<long> PostEventStatefulRecoveryRequest(string producerName, URN sportEventId, long requestId);

        Task PostRecoveryRequest(string producerName, long requestId, int nodeId, DateTime timestamp = default);

        TournamentInfoModel GetTournament(URN id, CultureInfo culture = null);

        teamExtended GetCompetitorProfile(URN id, CultureInfo culture);

        MatchSummaryModel GetMatchSummary(URN sportEventId, CultureInfo desiredCulture);

        FixturesEndpointModel GetFixture(URN id, CultureInfo culture);

        MatchStatusModel GetMatchStatusDescriptions(CultureInfo culture);

        ScheduleEndpointModel GetLiveMatches(CultureInfo culture);

        ScheduleEndpointModel GetMatches(DateTime dateToGet, CultureInfo culture);

        ScheduleEndpointModel GetSchedule(int startIndex, int limit, CultureInfo culture);

        fixtureChangesEndpoint GetFixtureChanges(CultureInfo culture);
        
        IObservable<T> SubscribeForClass<T>();

        Task<bool> PostReplayClear(int? nodeId);

        Task<bool> PostReplayStop(int? nodeId);

        Task<ReplayEndpointModel> GetReplaySetContent(int? nodeId);

        Task<bool> PutReplayEvent(URN eventId, int? nodeId);

        Task<bool> DeleteReplayEvent(URN eventId, int? nodeId);

        Task<bool> PostReplayStart(
            int? nodeId,
            int? speed = null,
            int? maxDelay = null,
            bool? useReplayTimestamp = null,
            string product = null,
            bool? runParallel = null);
    }
}
