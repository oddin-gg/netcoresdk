using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class Scoreboard : IScoreboard
{
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
        int? awayGoals,
        int? time,
        int? gameTime,
        int? currentDefenderTeam,
        // VirtualBasketballScoreboard
        // TableTennis
        int? homePoints,
        int? awayPoints,
        int? remainingGameTime,
        // eCricket
        int? homeRuns,
        int? awayRuns,
        int? homeWicketsFallen,
        int? awayWicketsFallen,
        int? homeOversPlayed,
        int? awayOversPlayed,
        int? homeBallsPlayed,
        int? awayBallsPlayed,
        bool? homeWonCoinToss,
        bool? homeBatting,
        bool? awayBatting,
        int? inning
    )
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
        Time = time;
        GameTime = gameTime;
        CurrentDefenderTeam = currentDefenderTeam;
        // VirtualBasketballScoreboard
        // TableTennis
        HomePoints = homePoints;
        AwayPoints = awayPoints;
        RemainingGameTime = remainingGameTime;
        // eCricket
        HomeRuns = homeRuns;
        AwayRuns = awayRuns;
        HomeWicketsFallen = homeWicketsFallen;
        AwayWicketsFallen = awayWicketsFallen;
        HomeOversPlayed = homeOversPlayed;
        AwayOversPlayed = awayOversPlayed;
        HomeBallsPlayed = homeBallsPlayed;
        AwayBallsPlayed = awayBallsPlayed;
        HomeWonCoinToss = homeWonCoinToss;
        HomeBatting = homeBatting;
        AwayBatting = awayBatting;
        Inning = inning;
    }

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
    public int? Time { get; }
    public int? GameTime { get; }

    public int? CurrentDefenderTeam { get; }

    // VirtualBasketballScoreboard
    public int? HomePoints { get; }
    public int? AwayPoints { get; }

    public int? RemainingGameTime { get; }

    // eCricket
    public int? HomeRuns { get; }
    public int? AwayRuns { get; }
    public int? HomeWicketsFallen { get; }
    public int? AwayWicketsFallen { get; }
    public int? HomeOversPlayed { get; }
    public int? AwayOversPlayed { get; }
    public int? HomeBallsPlayed { get; }
    public int? AwayBallsPlayed { get; }
    public bool? HomeWonCoinToss { get; }
    public bool? HomeBatting { get; }
    public bool? AwayBatting { get; }
    public int? Inning { get; }
}