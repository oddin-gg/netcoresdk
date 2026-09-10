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
    // Set: the check and the add are in different critical sections.
    private readonly HashSet<CultureInfo> _loadedLocales = new();

    private readonly Semaphore _semaphore = new(1, 1);

    private readonly IDisposable _subscription;

    public SportDataCache(IApiClient apiClient)
    {
        _apiClient = apiClient;
        _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
            .Subscribe(response =>
            {
                // An escape here kills the subscription for good.
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
            }).ToList();
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
            // Unlocked: publishing runs the sibling observers, which take their own locks.
            tournaments = _apiClient.GetTournaments(id, culture);
        }
        catch (Exception e)
        {
            _log.LogError($"Error while fetching sport tournaments {culture.TwoLetterISOLanguageName}: {e}");
            return null;
        }

        var tournamentIds = new List<URN>();

        _semaphore.WaitOne();
        try
        {
            foreach (var tournament in tournaments.tournaments)
            {
                // Per item: new URN throws on a malformed server id, and outside the
                // try it would abort the whole loop.
                try
                {
                    if (string.IsNullOrEmpty(tournament?.id))
                        continue;

                    tournamentIds.Add(new URN(tournament.id));
                }
                catch (Exception e)
                {
                    _log.LogError($"Failed to read sport tournament '{tournament?.id}': {e}");
                }
            }

            // Once, after the loop: a reader takes the reference without the
            // semaphore, so it must never see a half-filled set.
            RefreshOrInsertItem(id, culture, tournamentIds: tournamentIds);
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

    // Copy-on-write: the published entry is handed out by reference and read
    // outside the semaphore, so a visible set is never mutated. Returns current
    // untouched when there is nothing to add, so "unknown" never becomes "empty".
    private static ICollection<URN> WithTournaments(ICollection<URN> current, ICollection<URN> tournamentIds)
    {
        if (tournamentIds is null || tournamentIds.Count == 0)
            return current;

        // Ordered, not a set: ISport.Tournaments enumerates this, and a client would
        // otherwise get server order on the first call and hash order afterwards.
        var updated = current is null ? new List<URN>() : new List<URN>(current);
        var known = new HashSet<URN>(updated);
        foreach (var tournamentId in tournamentIds)
        {
            if (known.Add(tournamentId))
                updated.Add(tournamentId);
        }

        return updated;
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
        // Grouped so each sport is published once, however many of its tournaments
        // this response carries.
        var bySport = new Dictionary<string, (sport Sport, List<URN> Tournaments)>();

        foreach (var tournament in tournamentData)
        {
            // Per item, so one malformed id does not drop the rest of the response.
            try
            {
                if (string.IsNullOrEmpty(tournament.Key) || string.IsNullOrEmpty(tournament.Value?.id))
                    continue;

                var tournamentId = new URN(tournament.Key);
                var sportKey = new URN(tournament.Value.id).ToString();

                if (bySport.TryGetValue(sportKey, out var entry) == false)
                    entry = bySport[sportKey] = (tournament.Value, new List<URN>());

                entry.Tournaments.Add(tournamentId);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to read sport for tournament '{tournament.Key}': {e}");
            }
        }

        foreach (var entry in bySport)
        {
            try
            {
                RefreshOrInsertItem(
                    new URN(entry.Key),
                    culture,
                    entry.Value.Sport,
                    entry.Value.Tournaments);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to refresh or insert sport '{entry.Key}': {e}");
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
                // Unlocked: publishing runs the sibling observers, which take their own locks.
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

                _loadedLocales.Add(culture);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    private void RefreshOrInsertItem(
        URN id,
        CultureInfo culture,
        sport sport = null,
        ICollection<URN> tournamentIds = null)
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

        localizedSport.TournamentIds = WithTournaments(localizedSport.TournamentIds, tournamentIds);

        _cache.Set(id.ToString(), localizedSport, _cachePolicy);
    }
}