using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IMatchStatus : ICompetitionStatus
{
    IEnumerable<IPeriodScore> PeriodScores { get; }

    int? MatchStatusId { get; }

    ILocalizedNamedValue MatchStatus { get; }

    double? HomeScore { get; }

    double? AwayScore { get; }

    bool IsScoreboardAvailable { get; }

    IScoreboard Scoreboard { get; }

    ILocalizedNamedValue GetMatchStatus(CultureInfo culture);
}