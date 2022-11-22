using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class PeriodScore : IPeriodScore
    {
        public PeriodScore(
            double homeScore,
            double awayScore,
            int periodNumber,
            int matchStatusCode,
            string type,
            int? homeWonRounds,
            int? awayWonRounds,
            int? homeKills,
            int? awayKills,
            int? homeGoals,
            int? awayGoals,
            int? homePoints,
            int? awayPoints)
        {
            HomeScore = homeScore;
            AwayScore = awayScore;
            PeriodNumber = periodNumber;
            MatchStatusCode = matchStatusCode;
            Type = type;
            HomeWonRounds = homeWonRounds;
            AwayWonRounds = awayWonRounds;
            HomeKills = homeKills;
            AwayKills = awayKills;
            HomeGoals = homeGoals;
            AwayGoals = awayGoals;
            HomePoints = homePoints;
            AwayPoints = awayPoints;
        }

        public double HomeScore { get; }
        public double AwayScore { get; }
        public int PeriodNumber { get; }
        public int MatchStatusCode { get; }
        public string Type { get; }
        public int? HomeWonRounds { get; }
        public int? AwayWonRounds { get; }
        public int? HomeKills { get; }
        public int? AwayKills { get; }
        public int? HomeGoals { get; }
        public int? AwayGoals { get; }
        public int? HomePoints { get; }
        public int? AwayPoints { get; }
    }
}