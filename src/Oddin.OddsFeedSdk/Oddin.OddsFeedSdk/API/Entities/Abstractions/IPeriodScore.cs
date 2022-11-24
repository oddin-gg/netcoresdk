namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
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
    }
}