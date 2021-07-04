using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.API.Entities
{
    internal class SportEvent : ISportEvent
    {
        private readonly URN _id;
        private readonly IApiClient _apiClient;

        public URN Id => _id;

        public SportEvent(URN urn, IApiClient apiClient)
        {
            _id = urn;
            _apiClient = apiClient;
        }

        public async Task<string> GetNameAsync(CultureInfo culture)
            => (await _apiClient.GetMatchSummaryAsync(_id, culture)).Name;

        public async Task<DateTime?> GetScheduledTimeAsync()
            => (await _apiClient.GetMatchSummaryAsync(_id)).ScheduledTime;

        public async Task<DateTime?> GetScheduledEndTimeAsync()
            => (await _apiClient.GetMatchSummaryAsync(_id)).ScheduledEndTime;

        public async Task<URN> GetSportIdAsync()
            => (await _apiClient.GetMatchSummaryAsync(_id)).SportId;
    }
}
