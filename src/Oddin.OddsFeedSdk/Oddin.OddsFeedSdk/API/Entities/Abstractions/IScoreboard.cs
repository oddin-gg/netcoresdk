namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IScoreboard
{
    int? CurrentCtTeam { get; }
    int? HomeWonRounds { get; }
    int? AwayWonRounds { get; }
    int? CurrentRound { get; }
    int? HomeKills { get; }
    int? AwayKills { get; }
    int? HomeDestroyedTurrets { get; }
    int? AwayDestroyedTurrets { get; }
    int? HomeGold { get; }
    int? AwayGold { get; }
    int? HomeDestroyedTowers { get; }
    int? AwayDestroyedTowers { get; }
    int? HomeGoals { get; }
    int? AwayGoals { get; }
    int? Time { get; }
    int? GameTime { get; }

    int? CurrentDefenderTeam { get; }

    // VirtualBasketballScoreboard
    // TableTennis
    int? HomePoints { get; }
    int? AwayPoints { get; }

    int? RemainingGameTime { get; }

    // eCricket
    int? HomeRuns { get; }
    int? AwayRuns { get; }
    int? HomeWicketsFallen { get; }
    int? AwayWicketsFallen { get; }
    int? HomeOversPlayed { get; }
    int? AwayOversPlayed { get; }
    int? HomeBallsPlayed { get; }
    int? AwayBallsPlayed { get; }
    bool? HomeWonCoinToss { get; }
    bool? HomeBatting { get; }
    bool? AwayBatting { get; }
    int? Inning { get; }
}