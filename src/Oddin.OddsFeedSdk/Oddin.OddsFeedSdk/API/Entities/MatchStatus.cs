using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class MatchStatus : IMatchStatus
    {
        private readonly URN _sportEventId;
        private readonly IMatchStatusCache _matchStatusCache;
        private readonly ILocalizedStaticDataCache _dataCache;
        private readonly ExceptionHandlingStrategy _handlingStrategy;
        private readonly IEnumerable<CultureInfo> _cultures;

        public IEnumerable<IPeriodScore> PeriodScores => FetchMatchStatus()?.PeriodScores;

        public int? MatchStatusId => FetchMatchStatus()?.MatchStatusId;

        ILocalizedNamedValue IMatchStatus.MatchStatus => FetchLocalizedNameStatus(_cultures);

        public double? HomeScore => FetchMatchStatus()?.HomeScore;

        public double? AwayScore => FetchMatchStatus()?.AwayScore;

        public bool IsScoreboardAvailable => FetchMatchStatus()?.IsScoreboardAvailable ?? false;

        public IScoreboard Scoreboard => FetchMatchStatus()?.Scoreboard;

        public URN WinnerId => FetchMatchStatus()?.WinnerId;

        public EventStatus? Status => FetchMatchStatus()?.Status;

        public IReadOnlyDictionary<string, object> Properties => new ReadOnlyDictionary<string, object>(FetchMatchStatus()?.Properties);

        public MatchStatus(
            URN sportEventId,
            IMatchStatusCache matchStatusCache,
            ILocalizedStaticDataCache dataCache,
            ExceptionHandlingStrategy handlingStrategy,
            IEnumerable<CultureInfo> cultures)
        {
            _sportEventId = sportEventId;
            _matchStatusCache = matchStatusCache;
            _dataCache = dataCache;
            _handlingStrategy = handlingStrategy;
            _cultures = cultures;
        }

        public ILocalizedNamedValue GetMatchStatus(CultureInfo culture) => FetchLocalizedNameStatus(new[] { culture });

        private ILocalizedNamedValue FetchLocalizedNameStatus(IEnumerable<CultureInfo> cultures)
        {
            var statusId = MatchStatusId;
            if (statusId is null)
                return null;
            else
            {
                return _dataCache.Get(Convert.ToInt64(statusId.Value), cultures);
            }
        }

        private LocalizedMatchStatus FetchMatchStatus()
        {
            var item = _matchStatusCache.GetMatchStatus(_sportEventId);

            if (item is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(_sportEventId.ToString(), $"Match status for match {_sportEventId} not found");
            else
                return item;
        }
    }
}
