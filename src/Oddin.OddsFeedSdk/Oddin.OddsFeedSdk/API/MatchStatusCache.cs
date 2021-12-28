using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Messages;
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
        private readonly IAmqpClient _ampqClient;
        private readonly MemoryCache _cache = new(nameof(MatchStatusCache));

        private readonly Semaphore _semaphore = new(1, 1);
        private readonly TimeSpan _cacheTTL = TimeSpan.FromMinutes(20);
        private readonly IDisposable _subscription;

        public MatchStatusCache(IApiClient apiClient, IAmqpClient ampqClient)
        {
            _apiClient = apiClient;
            _ampqClient = ampqClient;
            _subscription = apiClient.SubscribeForClass<IRequestResult<object>>()
                .Subscribe(response =>
                {
                    if (response.Data is not MatchSummaryModel summary)
                        return;

                    var id = string.IsNullOrEmpty(summary?.sport_event?.id) ? null : new URN(summary.sport_event.id);

                    _semaphore.WaitOne();
                    try
                    {
                        _log.LogDebug($"Updating Match Status cache from API: {response.Data.GetType()}");
                        RefreshOrInsertApiItem(id, summary.sport_event_status);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });

            _ampqClient.OddsChangeMessageReceived += UpdateCacheOnOddsChange;
        }

        private void UpdateCacheOnOddsChange(object sender, SimpleMessageEventArgs<odds_change> e)
        {
            if (e.FeedMessage.sport_event_status == null)
                return;

            _log.LogDebug($"Updating Match Status cache from FEED for: {e.FeedMessage.event_id}");

            _semaphore.WaitOne();
            try
            {
                RefreshOrInsertFeedItem(string.IsNullOrEmpty(e?.FeedMessage?.event_id) ? null : new URN(e.FeedMessage.event_id), e.FeedMessage.sport_event_status);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to process message in match status cache");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void RefreshOrInsertFeedItem(URN id, AMQP.Messages.sportEventStatus status)
        {
            if (_cache.Get(id.ToString()) is not LocalizedMatchStatus item)
            {
                item = new LocalizedMatchStatus
                {
                    WinnerId = string.IsNullOrEmpty(status?.winner_id) ? null : new URN(status.winner_id),
                    Status = status.status.GetEventStatusFromFeed(),
                    PeriodScores = MapFeedPeriodScores(status.period_scores?.period_score ?? new periodScoreType[0]),
                    MatchStatusId = status.match_status,
                    HomeScore = status.home_score,
                    AwayScore = status.away_score,
                    IsScoreboardAvailable = status.scoreboard_available,
                    Scoreboard = MakeFeedScoreboard(status.scoreboard)
                };
            }
            else
            {
                item.WinnerId = string.IsNullOrEmpty(status?.winner_id) ? null : new URN(status.winner_id);
                item.Status = status.status.GetEventStatusFromFeed();
                item.PeriodScores = MapFeedPeriodScores(status.period_scores?.period_score ?? new periodScoreType[0]);
                item.MatchStatusId = status.match_status;
                item.HomeScore = status.home_score;
                item.AwayScore = status.away_score;
                item.IsScoreboardAvailable = status.scoreboard_available;

                if (status.scoreboard is not null)
                    item.Scoreboard = MakeFeedScoreboard(status.scoreboard);
            }

            _cache.Set(id.ToString(), item, _cacheTTL.AsCachePolicy());
        }

        private void RefreshOrInsertApiItem(URN id, Models.sportEventStatus summary)
        {
            if (_cache.Get(id.ToString()) is not LocalizedMatchStatus item)
            {
                item = new LocalizedMatchStatus
                {
                    WinnerId = string.IsNullOrEmpty(summary?.winner_id) ? null : new URN(summary.winner_id),
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
                item.WinnerId = string.IsNullOrEmpty(summary?.winner_id) ? null : new URN(summary.winner_id);
                item.Status = summary.status.GetEventStatusFromApi();
                item.PeriodScores = MapApiPeriodScores(summary.period_scores ?? new periodScore[0]);
                item.MatchStatusId = summary.match_status_code;
                item.HomeScore = summary.home_score;
                item.AwayScore = summary.away_score;
                item.IsScoreboardAvailable = summary.scoreboard_available;

                if (summary.scoreboard != null)
                    item.Scoreboard = MakeApiScoreboard(summary.scoreboard);
            }

            _cache.Set(id.ToString(), item, _cacheTTL.AsCachePolicy());
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
                scoreboard.away_destroyed_towers,
                scoreboard.home_goals,
                scoreboard.away_goals);
        }

        private Scoreboard MakeFeedScoreboard(scoreboard scoreboard)
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
                scoreboard.away_destroyed_towers,
                scoreboard.home_goals,
                scoreboard.away_goals);
        }

        private List<PeriodScore> MapApiPeriodScores(periodScore[] scores)
            => scores.Select(s =>
                new PeriodScore(
                    s.home_score,
                    s.away_score,
                    s.number,
                    s.match_status_code,
                    s.type,
                    s.home_won_rounds,
                    s.away_won_rounds,
                    s.home_kills,
                    s.away_kills,
                    s.home_goals,
                    s.away_goals))
                .OrderBy(s => s.PeriodNumber)
                .ToList();

        private List<PeriodScore> MapFeedPeriodScores(periodScoreType[] scores)
            => scores.Select(s =>
                new PeriodScore(
                    s.home_score,
                    s.away_score,
                    s.number,
                    s.match_status_code,
                    s.type,
                    s.home_won_rounds,
                    s.away_won_rounds,
                    s.home_kills,
                    s.away_kills,
                    s.home_goals,
                    s.away_goals))
                .OrderBy(s => s.PeriodNumber)
                .ToList();

        public LocalizedMatchStatus GetMatchStatus(URN id)
        {
            if (_cache.Get(id.ToString()) is not LocalizedMatchStatus matchStatus)
            {
                _apiClient.GetMatchSummary(id, Feed.AvailableLanguages().First());
                matchStatus = _cache.Get(id.ToString()) as LocalizedMatchStatus;
            }
            return matchStatus;
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }

        public void Dispose() => _subscription.Dispose();
    }
}