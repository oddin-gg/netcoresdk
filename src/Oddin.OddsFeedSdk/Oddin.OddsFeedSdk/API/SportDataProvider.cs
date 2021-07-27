using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal class SportDataProvider : ISportDataProvider
    {
        private readonly ICacheManager _cacheManager;
        private readonly IFeedConfiguration _feedConfiguration;
        private readonly ISportDataBuilder _builder;
        private readonly IExceptionWrapper _exceptionWrapper;
        private readonly IApiClient _apiClient;

        public SportDataProvider(ICacheManager cacheManager, IFeedConfiguration feedConfiguration, ISportDataBuilder builder, IExceptionWrapper exceptionWrapper, IApiClient apiClient)
        {
            _cacheManager = cacheManager;
            _feedConfiguration = feedConfiguration;
            _builder = builder;
            _exceptionWrapper = exceptionWrapper;
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
        {
            if (culture is null)
                culture = _feedConfiguration.DefaultLocale;

            return await _exceptionWrapper.Wrap(async () => await _builder.BuildSports(new[] { culture }));
        }

        public async Task<ISport> GetSportAsync(URN id, CultureInfo culture = null)
        {
            var sports = await GetSportsAsync(culture);
            return sports.FirstOrDefault(s => s.Id == id);
        }
        
        public IEnumerable<ITournament> GetActiveTournaments()
        {
            return GetActiveTournaments(_feedConfiguration.DefaultLocale);
        }

        public IEnumerable<ITournament> GetActiveTournaments(CultureInfo culture)
        {
            var sports = GetSportsAsync(culture).ConfigureAwait(false).GetAwaiter().GetResult();
            var sportTournaments = sports.SelectMany(s => s.Tournaments);

            return new HashSet<ITournament>(sportTournaments, new TournamentIdComparer());
        }

        public IEnumerable<ITournament> GetActiveTournaments(string name)
        {
            return GetActiveTournaments(name, _feedConfiguration.DefaultLocale);
        }

        public IEnumerable<ITournament> GetActiveTournaments(string name, CultureInfo culture)
        {
            var sports = GetSportsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var sport = sports?.FirstOrDefault(s => s.GetName(culture).Equals(name));
            return sport.Tournaments ?? new List<ITournament>();
        }

        public IEnumerable<ITournament> GetAvailableTournaments(URN sportId)
        {
            return GetAvailableTournaments(sportId, _feedConfiguration.DefaultLocale);
        }
        
        public IEnumerable<ITournament> GetAvailableTournaments(URN sportId, CultureInfo culture)
        {
            var result = _exceptionWrapper.Wrap(() => _apiClient.GetTournaments(sportId, culture));

            return result.tournament.Select(
                t => _builder.BuildTournament(new URN(t.id), sportId, new[] { culture }));
        }

        public void ClearTournament(URN id)
        {
            _cacheManager.TournamentsCache.ClearCacheItem(id);
        }

        public void ClearMatch(URN id)
        {
            // TODO: Implement
        }

        internal class TournamentIdComparer : IEqualityComparer<ITournament>
        {
            public bool Equals(ITournament one, ITournament two)
                => one.Id == two.Id;
        
            public int GetHashCode(ITournament item)
                => item.GetHashCode();
        }
    }
}
