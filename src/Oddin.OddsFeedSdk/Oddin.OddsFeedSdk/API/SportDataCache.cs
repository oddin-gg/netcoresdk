using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal class SportDataCache : ISportDataCache, IDisposable
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new MemoryCache(nameof(SportDataCache));
        private readonly IList<CultureInfo> _loadedLocales = new List<CultureInfo>();

        private readonly Semaphore _semaphore = new Semaphore(1, 1);

        public SportDataCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<URN>> GetSports(IEnumerable<CultureInfo> cultures)
        {
            _semaphore.WaitOne();
            try
            {
                var culturesToLoad = cultures.Except(_loadedLocales);
                if(culturesToLoad.Any())
                    await LoadAndCacheItem(culturesToLoad);

                return _cache.Select(item =>
                {
                    var sport = item.Value as LocalizedSport;
                    return sport.Id;
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<LocalizedSport> GetSport(URN id, IEnumerable<CultureInfo> cultures)
        {
            _semaphore.WaitOne();
            try
            {
                var localizedSport = _cache.Get(id.ToString()) as LocalizedSport;
                var sportCultures = localizedSport?.LoadedLocals ?? new List<CultureInfo>();
                var toLoadCultures = cultures.Except(sportCultures);
                if(toLoadCultures.Any())
                {
                    await LoadAndCacheItem(toLoadCultures);
                }

                return _cache.Get(id.ToString()) as LocalizedSport;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IEnumerable<URN> GetSportTournaments(URN id, CultureInfo culture)
        {
            _semaphore.WaitOne();
            try
            {
                TournamentsModel tournaments;
                try
                {
                    tournaments = _apiClient.GetTournaments(id, culture);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while fetching sport tournaments {culture.TwoLetterISOLanguageName}: {e}");
                    return null;
                }

                var tournamentIds = tournaments.tournament.Select(t => new URN(t.id));
                foreach (var tournamentId in tournamentIds)
                {
                    try
                    {
                        RefreshOrInsertItem(id, culture, tournamentId: tournamentId);
                    }
                    catch (Exception e)
                    {
                        _log.LogError($"Failed to insert or refresh sport tournaments: {e}");
                    }
                }
                return tournamentIds;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // TODO: handle tournament data from events

        private async Task LoadAndCacheItem(IEnumerable<CultureInfo> cultures)
        {
            foreach (var culture in cultures)
            {
                SportsModel sports;
                try
                {
                    sports = await _apiClient.GetSports(culture);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while fetching sports {culture.TwoLetterISOLanguageName}: {e}");
                    continue;
                }

                foreach(var sport in sports.sport)
                {
                    var id = new URN(sport.id);
                    try
                    {
                        RefreshOrInsertItem(id, culture, sport);
                    }
                    catch (Exception e)
                    {
                        _log.LogError($"Failed to insert or refresh sport: {e}");
                    }
                }
                _loadedLocales.Add(culture);
            }
        }

        private void RefreshOrInsertItem(URN id, CultureInfo culture, sportExtended sport = null, URN tournamentId = null)
        {
            var localizedSportItem = _cache.Get(id.ToString());

            LocalizedSport localizedSport;
            if (localizedSportItem is null)
                localizedSport = new LocalizedSport(id);
            else
                localizedSport = localizedSportItem as LocalizedSport;

            if (sport != null)
                localizedSport.Name[culture] = sport.name;
        
            if(tournamentId != null)
                localizedSport.TournamentIds ??= new List<URN>();

            var policy = new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromDays(1)
            };
            _cache.Set(id.ToString(), localizedSport, policy);
        }

        public void Dispose() => throw new NotImplementedException(); // Dispose subscribtion
    }
}
