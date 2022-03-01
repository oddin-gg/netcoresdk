using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API
{
    internal class MatchCache : IMatchCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(MatchCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new(nameof(MatchCache));
        private readonly Semaphore _semaphore = new(1, 1);
        private readonly TimeSpan _cacheTTL = TimeSpan.FromHours(12);
        private readonly IDisposable _subscription;

        public MatchCache(IApiClient apiClient)
        {
            _apiClient = apiClient;

            _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
                .Subscribe(response =>
                {
                    if (response.Culture is null || response.Data is null)
                        return;

                    var matches = response.Data switch
                    {
                        FixturesEndpointModel f => new List<sportEvent> { f.fixture },
                        ScheduleEndpointModel s => s.sport_event.ToList(),
                        TournamentScheduleModel t => t.sport_events.SelectMany(s => s).ToList(),
                        _ => new List<sportEvent>(),
                    };


                    if (matches.Any())
                    {
                        _semaphore.WaitOne();
                        try
                        {
                            _log.LogDebug($"Updating Match cache from API: {response.Data.GetType()}");
                            HandleMatchData(response.Culture, matches);
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
            var id = string.IsNullOrEmpty(e?.event_id)
                ? null
                : new URN(e.event_id);

            if (id?.Type == "match")
            {
                _log.LogDebug($"Invalidating Tournament cache from FEED for: {id}");
                _cache.Remove(id.ToString());
            }
        }

        public LocalizedMatch GetMatch(URN id, IEnumerable<CultureInfo> cultures)
        {
            _semaphore.WaitOne();
            try
            {
                var localizedMatch = _cache.Get(id.ToString()) as LocalizedMatch;
                var localizedAlready = localizedMatch?.LoadedLocals ?? new List<CultureInfo>();

                var culturesToLoad = cultures.Except(localizedAlready);
                if (culturesToLoad.Any())
                    LoadAndCacheItem(id, culturesToLoad);

                return _cache.Get(id.ToString()) as LocalizedMatch;
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
                MatchSummaryModel matchData;
                try
                {
                    matchData = _apiClient.GetMatchSummary(id, culture);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while fetching match summary {culture.TwoLetterISOLanguageName}: {e}");
                    continue;
                }
                try
                {
                    RefreshOrInsertItem(id, culture, matchData.sport_event);
                }
                catch (Exception e)
                {
                    _log.LogError($"Failed to refresh or load tournament {culture.TwoLetterISOLanguageName}: {e}");
                }
            }
        }

        private void RefreshOrInsertItem(URN id, CultureInfo culture, sportEvent model)
        {
            var homeTeamId = model.competitors?.FirstOrDefault()?.id;
            var homeTeamQualifier = model.competitors?.FirstOrDefault()?.qualifier;
            var awayTeamId = model.competitors?.LastOrDefault()?.id;
            var awayTeamQualifier = model.competitors?.LastOrDefault()?.qualifier;

            if (_cache.Get(id.ToString()) is LocalizedMatch item)
            {
                item.RefId = string.IsNullOrEmpty(model.refid) ? null : new URN(model.refid);
                item.ScheduledTime = model.scheduledSpecified ? model.scheduled : default(DateTime?);
                item.ScheduledEndTime = model.scheduled_endSpecified ? model.scheduled_end : default(DateTime?);
                item.SportId = string.IsNullOrEmpty(model.tournament?.sport?.id) ? null : new URN(model.tournament.sport.id);
                item.TournamentId = string.IsNullOrEmpty(model.tournament?.id) ? null : new URN(model.tournament.id);
                item.HomeTeamId = string.IsNullOrEmpty(homeTeamId) ? null : new URN(homeTeamId);
                item.AwayTeamId = string.IsNullOrEmpty(awayTeamId) ? null : new URN(awayTeamId);
                item.LiveOddsAvailability = model.liveodds.ParseToLiveOddsAvailability();
                item.HomeTeamQualifier = homeTeamQualifier;
                item.AwayTeamQualifier = awayTeamQualifier;
            }
            else
            {
                item = new LocalizedMatch(id)
                {
                    RefId = string.IsNullOrEmpty(model.refid) ? null : new URN(model.refid),
                    ScheduledTime = model.scheduledSpecified ? model.scheduled : default(DateTime?),
                    ScheduledEndTime = model.scheduled_endSpecified ? model.scheduled_end : default(DateTime?),
                    SportId = string.IsNullOrEmpty(model.tournament?.sport?.id) ? null : new URN(model.tournament.sport.id),
                    TournamentId = string.IsNullOrEmpty(model.tournament?.id) ? null : new URN(model.tournament.id),
                    HomeTeamId = string.IsNullOrEmpty(homeTeamId) ? null : new URN(homeTeamId),
                    AwayTeamId = string.IsNullOrEmpty(awayTeamId) ? null : new URN(awayTeamId),
                    LiveOddsAvailability = model.liveodds.ParseToLiveOddsAvailability(),
                    HomeTeamQualifier = homeTeamQualifier,
                    AwayTeamQualifier = awayTeamQualifier
                };
            }

            item.Name[culture] = model.name;

            _cache.Set(id.ToString(), item, _cacheTTL.AsCachePolicy());
        }

        private void HandleMatchData(CultureInfo culture, List<sportEvent> tournaments)
        {
            foreach(var tournament in tournaments)
            {
                var id = string.IsNullOrEmpty(tournament?.id) ? null : new URN(tournament.id);
                RefreshOrInsertItem(id, culture, tournament);
            }
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }

        public void Dispose() => _subscription.Dispose();
    }
}