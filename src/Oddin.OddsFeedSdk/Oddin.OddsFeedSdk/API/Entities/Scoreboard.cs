using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class Scoreboard : IScoreboard
    {
        public int? CurrentCtTeam { get; }
        public int? HomeWonRounds { get; }
        public int? AwayWonRounds { get; }
        public int? CurrentRound { get; }
        public int? HomeKills { get; }
        public int? AwayKills { get; }
        public int? HomeDestroyedTurrets { get; }
        public int? AwayDestroyedTurrets { get; }
        public int? HomeGold { get; }
        public int? AwayGold { get; }
        public int? HomeDestroyedTowers { get; }
        public int? AwayDestroyedTowers { get; }
        public int? HomeGoals { get; }
        public int? AwayGoals { get; }

        public Scoreboard(
            int? currentCtTeam,
            int? homeWonRounds,
            int? awayWonRounds,
            int? currentRound,
            int? homeKills,
            int? awayKills,
            int? homeDestroyedTurrets,
            int? awayDestroyedTurrets,
            int? homeGold,
            int? awayGold,
            int? homeDestroyedTowers,
            int? awayDestroyedTowers,
            int? homeGoals,
            int? awayGoals)
        {
            CurrentCtTeam = currentCtTeam;
            HomeWonRounds = homeWonRounds;
            AwayWonRounds = awayWonRounds;
            CurrentRound = currentRound;
            HomeKills = homeKills;
            AwayKills = awayKills;
            HomeDestroyedTurrets = homeDestroyedTurrets;
            AwayDestroyedTurrets = awayDestroyedTurrets;
            HomeGold = homeGold;
            AwayGold = awayGold;
            HomeDestroyedTowers = homeDestroyedTowers;
            AwayDestroyedTowers = awayDestroyedTowers;
            HomeGoals = homeGoals;
            AwayGoals = awayGoals;
        }
    }
}
