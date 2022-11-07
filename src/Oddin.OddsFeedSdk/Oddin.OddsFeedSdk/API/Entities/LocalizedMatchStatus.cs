using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedMatchStatus
{
    internal URN WinnerId { get; set; }
    internal EventStatus Status { get; set; }
    internal List<PeriodScore> PeriodScores { get; set; }
    internal int? MatchStatusId { get; set; }
    internal double HomeScore { get; set; }
    internal double AwayScore { get; set; }
    internal bool IsScoreboardAvailable { get; set; }
    internal Scoreboard Scoreboard { get; set; }
    internal IDictionary<string, object> Properties { get; set; }
}