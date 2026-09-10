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
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(12);

    private readonly Semaphore _semaphore = new(1, 1);
    private readonly IDisposable _subscription;

    public TournamentsCache(IApiClient apiClient)
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
                    _log.LogError(e, "Failed to side-load tournaments");
                }
            });
    }

    private void HandleResponse(IRequestResult<object> response)
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

        if (tournaments.Any() == false)
            return;

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
        var culturesToLoad = MissingCultures(id, cultures);
        if (culturesToLoad.Count > 0)
            LoadAndCacheItem(id, culturesToLoad);

        return Read(id);
    }

    public IEnumerable<URN> GetTournamentCompetitors(URN id, CultureInfo culture)
    {
        LoadAndCacheItem(id, new[] { culture });
        return Read(id)?.CompetitorIds;
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

    private List<CultureInfo> MissingCultures(URN id, IEnumerable<CultureInfo> cultures)
    {
        _semaphore.WaitOne();
        try
        {
            var localizedTournament = _cache.Get(id.ToString()) as LocalizedTournament;
            var localizedAlready = localizedTournament?.LoadedLocals ?? new List<CultureInfo>();
            return cultures.Except(localizedAlready).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private LocalizedTournament Read(URN id)
    {
        _semaphore.WaitOne();
        try
        {
            return _cache.Get(id.ToString()) as LocalizedTournament;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            TournamentInfoModel tournamentData;
            try
            {
                // Deliberately outside the semaphore. The response is published on this
                // thread, so a sibling cache's side-load observer runs here and takes its
                // own lock; holding ours across the call lets two caches wait on each other.
                tournamentData = _apiClient.GetTournament(id, culture);
            }
            catch (Exception e)
            {
                _log.LogError($"Error while fetching tournament {culture.TwoLetterISOLanguageName}: {e}");
                continue;
            }

            _semaphore.WaitOne();
            try
            {
                RefreshOrInsertItem(id, culture, tournamentData.tournament);
            }
            catch (Exception e)
            {
                _log.LogError($"Failed to refresh or load tournament {culture.TwoLetterISOLanguageName}: {e}");
            }
            finally
            {
                _semaphore.Release();
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

        _cache.Set(id.ToString(), item, _cacheTtl.AsCachePolicy());
    }
}