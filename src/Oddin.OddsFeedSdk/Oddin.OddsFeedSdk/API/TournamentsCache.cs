using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API;

internal class TournamentsCache : ITournamentsCache
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(TournamentsCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(TournamentsCache));
    private readonly TimeSpan _cacheTTL = TimeSpan.FromHours(12);

    private readonly Semaphore _semaphore = new(1, 1);
    private readonly IDisposable _subscription;

    public TournamentsCache(IApiClient apiClient)
    {
        _apiClient = apiClient;

        _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
            .Subscribe(response =>
            {
                if (response.Culture is null || response.Data is null)
                    return;

                var tournaments = response.Data switch
                {
                    FixturesEndpointModel f => new[] { f.fixture.tournament },
                    TournamentsModel t => t.tournaments?.ToArray() ?? Array.Empty<tournament>(),
                    MatchSummaryModel m => new[] { m.sport_event.tournament },
                    ScheduleEndpointModel s => s.sport_event.Select(t => t.tournament).ToArray(),
                    TournamentScheduleModel t => t.tournament.ToArray(),
                    SportTournamentsModel s => s.tournaments?.ToArray() ?? Array.Empty<tournament>(),
                    _ => Array.Empty<tournament>()
                };


                if (tournaments.Any())
                {
                    _semaphore.WaitOne();
                    try
                    {
                        _log.LogDebug($"Updating Tournament cache from API: {response.Data.GetType()}");
                        HandleTournamentsData(response.Culture, tournaments);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            });
    }

    public void OnFeedMessageReceived(fixture_change e)
    {
        var id = string.IsNullOrEmpty(e?.event_id) ? null : new URN(e.event_id);

        if (id != null)
        {
            _log.LogDebug($"Invalidating Tournament cache from FEED for: {id}");
            _cache.Remove(id.ToString());
        }
    }

    public LocalizedTournament GetTournament(URN id, IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            var localizedTournament = _cache.Get(id.ToString()) as LocalizedTournament;
            var localizedAlready = localizedTournament?.LoadedLocals ?? new List<CultureInfo>();

            var culturesToLoad = cultures.Except(localizedAlready);
            if (culturesToLoad.Any())
                LoadAndCacheItem(id, culturesToLoad);

            return _cache.Get(id.ToString()) as LocalizedTournament;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public IEnumerable<URN> GetTournamentCompetitors(URN id, CultureInfo culture)
    {
        _semaphore.WaitOne();
        try
        {
            LoadAndCacheItem(id, new[] { culture });
            var tournament = _cache.Get(id.ToString()) as LocalizedTournament;
            return tournament?.CompetitorIds;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void ClearCacheItem(URN id) => _cache.Remove(id.ToString());

    public void Dispose() => _subscription.Dispose();

    private void HandleTournamentsData(CultureInfo culture, tournament[] tournaments)
    {
        foreach (var tournament in tournaments)
        {
            var id = string.IsNullOrEmpty(tournament?.id) ? null : new URN(tournament.id);

            try
            {
                RefreshOrInsertItem(id, culture, tournament);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to refresh or load tournament");
            }
        }
    }

    private void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            TournamentInfoModel tournamentData;
            try
            {
                tournamentData = _apiClient.GetTournament(id, culture);
            }
            catch (Exception e)
            {
                _log.LogError($"Error while fetching tournament {culture.TwoLetterISOLanguageName}: {e}");
                continue;
            }

            try
            {
                RefreshOrInsertItem(id, culture, tournamentData.tournament);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to refresh or load tournament {culture.TwoLetterISOLanguageName}: {e}");
            }
        }
    }

    private void RefreshOrInsertItem(URN id, CultureInfo culture, tournament model)
    {
        if (_cache.Get(id.ToString()) is LocalizedTournament item)
        {
            item.RefId = string.IsNullOrEmpty(model.refid) ? null : new URN(model.refid);
            item.StartDate = model.tournament_length?.start_date;
            item.EndDate = model.tournament_length?.end_date;
            item.SportId = string.IsNullOrEmpty(model.sport?.id) ? null : new URN(model.sport.id);
            item.ScheduledTime = model.scheduled;
            item.ScheduledEndTime = model.scheduled_end;
            item.RiskTier = model.riskTier;
        }
        else
        {
            item = new LocalizedTournament(id)
            {
                RefId = string.IsNullOrEmpty(model.refid) ? null : new URN(model.refid),
                StartDate = model.tournament_length?.start_date,
                EndDate = model.tournament_length?.end_date,
                SportId = string.IsNullOrEmpty(model.sport?.id) ? null : new URN(model.sport.id),
                ScheduledTime = model.scheduled,
                ScheduledEndTime = model.scheduled_end,
                RiskTier = model.riskTier
            };
        }

        item.Name[culture] = model.name;

        if (model is tournamentExtended modelExtended)
        {
            item.IconPath = modelExtended.icon_path;

            if (modelExtended.competitors.Any())
            {
                var ids = modelExtended.competitors.Select(c => string.IsNullOrEmpty(c?.id) ? null : new URN(c.id));
                var alreadyExistingIds = item.CompetitorIds ??= new HashSet<URN>();
                var alreadyExistingIdsHashSet = alreadyExistingIds.ToHashSet();

                foreach (var competitorId in ids)
                    alreadyExistingIdsHashSet.Add(competitorId);

                item.CompetitorIds = alreadyExistingIdsHashSet.ToList();
            }
        }

        _cache.Set(id.ToString(), item, _cacheTTL.AsCachePolicy());
    }
}