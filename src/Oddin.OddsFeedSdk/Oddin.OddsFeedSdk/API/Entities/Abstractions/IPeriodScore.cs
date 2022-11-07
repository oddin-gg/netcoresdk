namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IPeriodScore
{
    double HomeScore { get; }
    double AwayScore { get; }
    int PeriodNumber { get; }
    int MatchStatusCode { get; }
    string Type { get; }
    int? HomeWonRounds { get; }
    int? AwayWonRounds { get; }
    int? HomeKills { get; }
    int? AwayKills { get; }
    int? HomeGoals { get; }
    int? AwayGoals { get; }
    int? HomePoints { get; }
    int? AwayPoints { get; }
    int? HomeRuns { get; }
    int? AwayRuns { get; }
    int? HomeWicketsFallen { get; }
    int? AwayWicketsFallen { get; }
    int? HomeOversPlayed { get; }
    int? AwayOversPlayed { get; }
    int? HomeBallsPlayed { get; }
    int? AwayBallsPlayed { get; }
    bool? HomeWonCoinToss { get; }
}