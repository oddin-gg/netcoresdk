using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
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
    internal class MatchCache : IMatchCache, IDisposable
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(MatchCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new MemoryCache(nameof(MatchCache));
        private readonly Semaphore _semaphore = new Semaphore(1, 1);
        private readonly IDisposable _subscription;

        public MatchCache(IApiClient apiClient)
        {
            _apiClient = apiClient;

            _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
                .Subscribe(response =>
                {
                    if (response.Culture is null || response.Data is null)
                        return;

                    var tournaments = response.Data switch
                    {
                        FixturesEndpointModel f => new List<sportEvent> { f.fixture },
                        ScheduleEndpointModel s => s.sport_event.ToList(),
                        TournamentScheduleModel t => t.sport_events.SelectMany(s => s).ToList(),
                        _ => new List<sportEvent>(),
                    };

                    if (tournaments.Any())
                    {
                        _semaphore.WaitOne();
                        try
                        {
                            HandleMatchData(response.Culture, tournaments);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }
                });
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

        internal void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
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

        internal void RefreshOrInsertItem(URN id, CultureInfo culture, sportEvent model)
        {
            var homeTeamId = model.competitors?.FirstOrDefault()?.id;
            var homeTeamQualifier = model.competitors?.FirstOrDefault()?.qualifier;
            var awayTeamId = model.competitors?.LastOrDefault()?.id;
            var awayTeamQualifier = model.competitors?.LastOrDefault()?.qualifier;

            if (_cache.Get(id.ToString()) is LocalizedMatch item)
            {
                item.ScheduledTime = model.scheduledSpecified ? model.scheduled : default(DateTime?);
                item.ScheduledEndTime = model.scheduled_endSpecified ? model.scheduled_end : default(DateTime?);
                item.SportId = new URN(model.tournament.sport.id);
                item.TournamentId = new URN(model.tournament.id);
                item.HomeTeamId = homeTeamId != null ? new URN(homeTeamId) : null;
                item.AwayTeamId = awayTeamId != null ? new URN(awayTeamId) : null;
                item.LiveOddsAvailability = model.liveodds.ParseToLiveOddsAvailability();
                item.HomeTeamQualifier = homeTeamQualifier;
                item.AwayTeamQualifier = awayTeamQualifier;
            }
            else
            {
                item = new LocalizedMatch(id)
                {
                    ScheduledTime = model.scheduledSpecified ? model.scheduled : default(DateTime?),
                    ScheduledEndTime = model.scheduled_endSpecified ? model.scheduled_end : default(DateTime?),
                    SportId = new URN(model.tournament.sport.id),
                    TournamentId = new URN(model.tournament.id),
                    HomeTeamId = homeTeamId != null ? new URN(homeTeamId) : null,
                    AwayTeamId = awayTeamId != null ? new URN(awayTeamId) : null,
                    LiveOddsAvailability = model.liveodds.ParseToLiveOddsAvailability(),
                    HomeTeamQualifier = homeTeamQualifier,
                    AwayTeamQualifier = awayTeamQualifier
                };
            }

            item.Name[culture] = model.name;

            _cache.Set(id.ToString(), item, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(12) });
        }

        private void HandleMatchData(CultureInfo culture, List<sportEvent> tournaments)
        {
            foreach(var tournament in tournaments)
            {
                var id = new URN(tournament.id);
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
