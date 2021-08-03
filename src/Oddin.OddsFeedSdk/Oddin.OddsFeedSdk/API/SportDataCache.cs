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
    internal class SportDataCache : ISportDataCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new MemoryCache(nameof(SportDataCache));
        private readonly IList<CultureInfo> _loadedLocales = new List<CultureInfo>();

        private readonly Semaphore _semaphore = new Semaphore(1, 1);
        private readonly TimeSpan _cacheTTL = TimeSpan.FromDays(1);

        private readonly IDisposable _subscription;

        public SportDataCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
                .Subscribe(response =>
                {
                    if (response.Culture is null || response.Data is null)
                        return;

                    var tournamentData = response.Data switch
                    {
                        TournamentScheduleModel t => t.tournament.ToDictionary(t => t.id, t => t.sport),
                        TournamentInfoModel t => new Dictionary<string, sport> { { t.tournament.id, t.tournament.sport } },
                        _ => new Dictionary<string, sport>()
                    };

                    if (tournamentData.Any())
                    {
                        _semaphore.WaitOne();
                        try
                        {
                            HandleTournamentData(response.Culture, tournamentData);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }
                });
        }

        private void HandleTournamentData(CultureInfo culture, Dictionary<string, sport> tournamentData)
        {
            foreach (var tournament in tournamentData)
            {
                var tournamentId = new URN(tournament.Key);
                var sportId = new URN(tournament.Key);

                RefreshOrInsertItem(sportId, culture, tournament.Value);
                var sport = _cache.Get(sportId.ToString()) as LocalizedSport;
                if (sport is not null)
                {
                    var sportTournaments = sport.TournamentIds.ToList();
                    sportTournaments.Add(tournamentId);
                    sport.TournamentIds = sportTournaments;
                }
            }
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

        private void RefreshOrInsertItem(URN id, CultureInfo culture, sport sport = null, URN tournamentId = null)
        {
            var localizedSportItem = _cache.Get(id.ToString());

            LocalizedSport localizedSport;
            if (localizedSportItem is null)
                localizedSport = new LocalizedSport(id);
            else
                localizedSport = localizedSportItem as LocalizedSport;

            if (sport != null)
            {
                localizedSport.RefId = string.IsNullOrEmpty(sport?.refid) ? null : new URN(sport.refid);
                localizedSport.Name[culture] = sport.name;
            }
        
            if(tournamentId != null)
                localizedSport.TournamentIds ??= new List<URN>();

            _cache.Set(id.ToString(), localizedSport, _cacheTTL.AsCachePolicy());
        }

        public void Dispose() => _subscription.Dispose();
    }
}
