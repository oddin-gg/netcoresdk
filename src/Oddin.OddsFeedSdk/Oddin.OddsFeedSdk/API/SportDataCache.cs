using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API;

internal class SportDataCache : ISportDataCache
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(SportDataCache));
    private readonly CacheItemPolicy _cachePolicy = new() { Priority = CacheItemPriority.NotRemovable };
    // A set, not a list: the check and the add sit in different critical sections, so two
    // concurrent cold loads of the same culture can both reach the add.
    private readonly HashSet<CultureInfo> _loadedLocales = new();

    private readonly Semaphore _semaphore = new(1, 1);

    private readonly IDisposable _subscription;

    public SportDataCache(IApiClient apiClient)
    {
        _apiClient = apiClient;
        _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
            .Subscribe(response =>
            {
                // Everything is guarded: an exception escaping here disposes this
                // subscription for good, and Subject also rethrows it into the API caller
                // and skips every cache that subscribed after this one.
                try
                {
                    HandleResponse(response);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to side-load sports");
                }
            });
    }

    private void HandleResponse(IRequestResult<object> response)
    {
        if (response.Culture is null || response.Data is null)
            return;

        var tournamentData = response.Data switch
        {
            TournamentScheduleModel t => t.tournament.ToDictionary(t => t.id, t => t.sport),
            TournamentInfoModel t => new Dictionary<string, sport> { { t.tournament.id, t.tournament.sport } },
            _ => new Dictionary<string, sport>()
        };

        if (tournamentData.Any() == false)
            return;

        _semaphore.WaitOne();
        try
        {
            _log.LogDebug($"Updating SportData cache from API: {response.Data.GetType()}");
            HandleTournamentData(response.Culture, tournamentData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<URN>> GetSports(IEnumerable<CultureInfo> cultures)
    {
        var culturesToLoad = MissingLocales(cultures);
        if (culturesToLoad.Count > 0)
            await LoadAndCacheItem(culturesToLoad);

        _semaphore.WaitOne();
        try
        {
            return _cache.Select(item =>
            {
                var sport = item.Value as LocalizedSport;
                return sport?.Id;
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<LocalizedSport> GetSport(URN id, IEnumerable<CultureInfo> cultures)
    {
        var toLoadCultures = MissingCultures(id, cultures);
        if (toLoadCultures.Count > 0)
            await LoadAndCacheItem(toLoadCultures);

        return Read(id);
    }

    public IEnumerable<URN> GetSportTournaments(URN id, CultureInfo culture)
    {
        TournamentsModel tournaments;
        try
        {
            // Deliberately outside the semaphore. The response is published on this
            // thread, so a sibling cache's side-load observer runs here and takes its
            // own lock; holding ours across the call lets two caches wait on each other.
            tournaments = _apiClient.GetTournaments(id, culture);
        }
        catch (Exception e)
        {
            _log.LogError($"Error while fetching sport tournaments {culture.TwoLetterISOLanguageName}: {e}");
            return null;
        }

        var tournamentIds = tournaments.tournaments.Select(t => string.IsNullOrEmpty(t?.id) ? null : new URN(t.id));

        // One critical section for the whole loop: the body makes no API call, so keeping
        // it in one keeps the entry from being observed half-updated and avoids a kernel
        // transition per tournament.
        _semaphore.WaitOne();
        try
        {
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
        }
        finally
        {
            _semaphore.Release();
        }

        return tournamentIds;
    }

    private List<CultureInfo> MissingLocales(IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            return cultures.Except(_loadedLocales).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private List<CultureInfo> MissingCultures(URN id, IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            var localizedSport = _cache.Get(id.ToString()) as LocalizedSport;
            var sportCultures = localizedSport?.LoadedLocals ?? new List<CultureInfo>();
            return cultures.Except(sportCultures).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private LocalizedSport Read(URN id)
    {
        _semaphore.WaitOne();
        try
        {
            return _cache.Get(id.ToString()) as LocalizedSport;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose() => _subscription.Dispose();

    private void HandleTournamentData(CultureInfo culture, Dictionary<string, sport> tournamentData)
    {
        foreach (var tournament in tournamentData)
        {
            var tournamentId = string.IsNullOrEmpty(tournament.Key) ? null : new URN(tournament.Key);
            var sportId = string.IsNullOrEmpty(tournament.Key) ? null : new URN(tournament.Value.id);

            if (sportId is null) continue;

            RefreshOrInsertItem(sportId, culture, tournament.Value);
            var sport = _cache.Get(sportId.ToString()) as LocalizedSport;
            if (sport is not null)
            {
                var sportTournaments = sport.TournamentIds ??= new List<URN>();
                sportTournaments.Add(tournamentId);
                sport.TournamentIds = sportTournaments;
            }
        }
    }

    private async Task LoadAndCacheItem(IEnumerable<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            SportsModel sports;
            try
            {
                // Deliberately outside the semaphore. The sibling caches' side-load
                // observers run before this call completes, and they take their own locks;
                // holding ours across the call lets two caches wait on each other.
                sports = await _apiClient.GetSports(culture);
            }
            catch (Exception e)
            {
                _log.LogError($"Error while fetching sports {culture.TwoLetterISOLanguageName}: {e}");
                continue;
            }

            _semaphore.WaitOne();
            try
            {
                foreach (var sport in sports.sport)
                {
                    var id = string.IsNullOrEmpty(sport?.id) ? null : new URN(sport.id);
                    try
                    {
                        RefreshOrInsertItem(id, culture, sport);
                    }
                    catch (Exception e)
                    {
                        _log.LogError($"Failed to insert or refresh sport: {e}");
                    }
                }

                // Plain List — only ever read and written under the semaphore.
                _loadedLocales.Add(culture);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    private void RefreshOrInsertItem(URN id, CultureInfo culture, sport sport = null, URN tournamentId = null)
    {
        var localizedSportItem = _cache.Get(id.ToString());

        var localizedSport = localizedSportItem as LocalizedSport ?? new LocalizedSport(id);
        if (sport != null)
        {
            localizedSport.RefId = string.IsNullOrEmpty(sport.refid) ? null : new URN(sport.refid);
            localizedSport.Name[culture] = sport.name;
        }

        if (sport is sportExtended sportExtended)
            localizedSport.IconPath = sportExtended.icon_path;

        if (tournamentId != null)
            localizedSport.TournamentIds ??= new List<URN>();

        _cache.Set(id.ToString(), localizedSport, _cachePolicy);
    }
}