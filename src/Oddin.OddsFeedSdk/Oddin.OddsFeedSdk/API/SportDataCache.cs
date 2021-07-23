using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal class SportDataCache : ISportDataCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache;
        private readonly IList<CultureInfo> _loadedLocales = new List<CultureInfo>();

        private readonly SemaphoreSlim _loadAndCacheItemSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _getSemaphore = new SemaphoreSlim(1, 1);

        public SportDataCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<IEnumerable<URN>> GetSports(IEnumerable<CultureInfo> cultures)
        {
            await _getSemaphore.WaitAsync();
            try
            {
                var culturesToLoad = cultures.Except(_loadedLocales);
                if(culturesToLoad.Any())
                    await LoadAndCacheItem(culturesToLoad);

                return _cache.GetKeys<URN>().Select(key =>
                {
                    var sport = _cache.Get<LocalizedSport>(key);
                    return sport.Id;
                });
            }
            finally
            {
                _getSemaphore.Release();
            }
        }

        public async Task<LocalizedSport> GetSport(URN id, IEnumerable<CultureInfo> cultures)
        {
            await _getSemaphore.WaitAsync();
            try
            {
                var localizedSport = _cache.Get<LocalizedSport>(id);
                var sportCultures = localizedSport?.LoadedLocals ?? new List<CultureInfo>();
                var toLoadCultures = cultures.Except(sportCultures);
                if(toLoadCultures.Any())
                {
                    await LoadAndCacheItem(toLoadCultures);
                }

                return _cache.Get<LocalizedSport>(id);
            }
            finally
            {
                _getSemaphore.Release();
            }
        }

        public IEnumerable<URN> GetSportTournaments(URN id, CultureInfo culture)
        {
            _getSemaphore.Wait();
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
                _getSemaphore.Release();
            }
        }

        // TODO: handle tournament data from events

        private async Task LoadAndCacheItem(IEnumerable<CultureInfo> cultures)
        {
            _loadAndCacheItemSemaphore.Wait();
            try
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
            finally
            {
                _loadAndCacheItemSemaphore.Release();
            }
        }

        private void RefreshOrInsertItem(URN id, CultureInfo culture, sportExtended sport = null, URN tournamentId = null)
        {
            var isInCache = _cache.TryGetValue<LocalizedSport>(id, out var localizedSport);

            if (isInCache == false)
                localizedSport = new LocalizedSport(id);

            if (sport != null)
                localizedSport.Name[culture] = sport.name;
        
            if(tournamentId != null)
                localizedSport.TournamentIds ??= new List<URN>();
           
            _cache.Set(id, localizedSport, TimeSpan.FromDays(1));
        }
    }
}
