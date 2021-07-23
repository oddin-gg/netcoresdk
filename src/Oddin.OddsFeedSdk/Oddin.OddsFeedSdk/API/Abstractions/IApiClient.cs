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

        Task<IEnumerable<IMarketDescription>> GetMarketDescriptionsAsync(CultureInfo culture = null);

        Task<long> PostEventRecoveryRequest(string producerName, URN sportEventId, long requestId);

        Task<long> PostEventStatefulRecoveryRequest(string producerName, URN sportEventId, long requestId);

        Task PostRecoveryRequest(string producerName, long requestId, int nodeId, DateTime timestamp = default);
    }
}
