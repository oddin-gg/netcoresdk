using System;
using System.Collections.Generic;
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
    internal class MatchStatusCache : IMatchStatusCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(MatchStatusCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new MemoryCache(nameof(MatchStatusCache));

        private readonly Semaphore _semaphore = new Semaphore(1, 1);
        private readonly IDisposable _subscription;

        public MatchStatusCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
                .Subscribe(response =>
                {
                    if (response.Data is not MatchSummaryModel summary)
                        return;

                    var id = new URN(summary.sport_event.id);

                    _semaphore.WaitOne();
                    try
                    {
                        RefreshOrInsertApiItem(id, summary.sport_event_status);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });
        }

        private void RefreshOrInsertApiItem(URN id, sportEventStatus summary)
        {
            var item = _cache.Get(id.ToString()) as LocalizedMatchStatus;

            if (item is null)
            {
                item = new LocalizedMatchStatus
                {
                    WinnerId = summary.winner_id is null ? null : new URN(summary.winner_id),
                    Status = summary.status.GetEventStatusFromApi(),
                    PeriodScores = MapApiPeriodScores(summary.period_scores ?? new periodScore[0]),
                    MatchStatusId = summary.match_status_code,
                    HomeScore = summary.home_score,
                    AwayScore = summary.away_score,
                    IsScoreboardAvailable = summary.scoreboard_available,
                    Scoreboard = MakeApiScoreboard(summary.scoreboard)
                };
            }
            else
            {
                item.WinnerId = summary.winner_id is null ? null : new URN(summary.winner_id);
                item.Status = summary.status.GetEventStatusFromApi();
                item.PeriodScores = MapApiPeriodScores(summary.period_scores ?? new periodScore[0]);
                item.MatchStatusId = summary.match_status_code;
                item.HomeScore = summary.home_score;
                item.AwayScore = summary.away_score;
                item.IsScoreboardAvailable = summary.scoreboard_available;

                if (summary.scoreboard != null)
                    item.Scoreboard = MakeApiScoreboard(summary.scoreboard);
            }

            _cache.Set(id.ToString(), item, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(20) });
        }

        private Scoreboard MakeApiScoreboard(ScoreboardModel scoreboard)
        {
            if (scoreboard is null)
                return null;

            return new Scoreboard(
                scoreboard.current_ct_team,
                scoreboard.home_won_rounds,
                scoreboard.away_won_rounds,
                scoreboard.current_round,
                scoreboard.home_kills,
                scoreboard.away_kills,
                scoreboard.home_destroyed_turrets,
                scoreboard.away_destroyed_turrets,
                scoreboard.home_gold,
                scoreboard.away_gold,
                scoreboard.home_destroyed_towers,
                scoreboard.away_destroyed_towers);
        }

        private List<PeriodScore> MapApiPeriodScores(periodScore[] scores)
            => scores.Select(s =>
                new PeriodScore(
                    s.home_score,
                    s.away_score,
                    s.number,
                    s.match_status_code,
                    s.home_won_rounds,
                    s.away_won_rounds,
                    s.home_kills,
                    s.away_kills))
                .OrderBy(s => s.PeriodNumber)
                .ToList();

        public LocalizedMatchStatus GetMatchStatus(URN id)
        {
            _semaphore.WaitOne();
            try
            {
                var matchStatus = _cache.Get(id.ToString()) as LocalizedMatchStatus;
                if (matchStatus == null)
                {
                    _apiClient.GetMatchSummary(id, Feed.AvailableLanguages().First());
                    matchStatus = _cache.Get(id.ToString()) as LocalizedMatchStatus;
                }
                return matchStatus;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }


        //TODO
        public void Dispose() => _subscription.Dispose();
    }
}
