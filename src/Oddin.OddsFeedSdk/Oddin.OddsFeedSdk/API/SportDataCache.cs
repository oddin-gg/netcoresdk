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
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.API;

internal class SportDataCache : ISportDataCache
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(SportDataCache));
    private readonly CacheItemPolicy _cachePolicy = new() { Priority = CacheItemPriority.NotRemovable };
    private readonly IFeedConfiguration _config;
    private readonly IList<CultureInfo> _loadedLocales = new List<CultureInfo>();

    private readonly Semaphore _semaphore = new(1, 1);

    private readonly IDisposable _subscription;

    public SportDataCache(
        IApiClient apiClient,
        IFeedConfiguration config
    )
    {
        _apiClient = apiClient;
        _config = config;
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
                        _log.LogDebug("Updating SportData cache from API: {Type}", response.Data.GetType());
                        HandleTournamentData(response.Culture, tournamentData);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            });
    }

    public async Task<IEnumerable<URN>> GetSports(IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            var culturesToLoad = cultures.Except(_loadedLocales);
            if (culturesToLoad.Any())
                await LoadAndCacheItem(culturesToLoad);

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
        _semaphore.WaitOne();
        try
        {
            var localizedSport = _cache.Get(id.ToString()) as LocalizedSport;
            var sportCultures = localizedSport?.LoadedLocals ?? new List<CultureInfo>();
            var toLoadCultures = cultures.Except(sportCultures);
            if (toLoadCultures.Any())
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
                _log.LogError("Error while fetching sport tournaments {CultureTwoLetterIsoLanguageName}: {E}",
                    culture.TwoLetterISOLanguageName, e);
                e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                return null;
            }

            var tournamentIds = tournaments.tournaments.Select(t => string.IsNullOrEmpty(t?.id) ? null : new URN(t.id));
            foreach (var tournamentId in tournamentIds)
            {
                try
                {
                    RefreshOrInsertItem(id, culture, tournamentId: tournamentId);
                }
                catch (Exception e)
                {
                    _log.LogError("Failed to insert or refresh sport tournaments: {E}", e);
                    e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                }
            }

            return tournamentIds;
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
                sports = await _apiClient.GetSports(culture);
            }
            catch (Exception e)
            {
                _log.LogError("Error while fetching sports {CultureTwoLetterIsoLanguageName}: {E}",
                    culture.TwoLetterISOLanguageName, e);
                e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                continue;
            }

            foreach (var sport in sports.sport)
            {
                var id = string.IsNullOrEmpty(sport?.id) ? null : new URN(sport.id);
                try
                {
                    RefreshOrInsertItem(id, culture, sport);
                }
                catch (Exception e)
                {
                    _log.LogError("Failed to insert or refresh sport: {E}", e);
                    e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                }
            }

            _loadedLocales.Add(culture);
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