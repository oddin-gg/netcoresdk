using System.IO;
using System.Xml.Serialization;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Models;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests;

// Set-based classic sports report the games of each set on the period row,
// next to the running sets-won tally (home_score/away_score). Before games
// were modelled on period_score, XmlSerializer silently dropped the attribute
// and a consumer rendering per-set rows showed the tally (1:0) where the set
// was actually 6:4. The scoreboard cannot recover it either - its games reset
// with every new set. The Specified flags drive the null mapping in
// MatchStatusCache, keeping absence distinguishable from a real 0:0.
public class PeriodScoreGamesTests
{
    [Fact]
    public void FeedPeriodScoresCarryGames()
    {
        const string raw = @"<odds_change event_id=""od:match:11"" product=""1"" timestamp=""1785831336922"">
              <sport_event_status status=""1"" match_status=""201"" home_score=""1"" away_score=""0"">
                <period_scores>
                  <period_score type=""set"" number=""1"" match_status_code=""200"" home_score=""1"" away_score=""0"" home_games=""6"" away_games=""4""/>
                  <period_score type=""set"" number=""2"" match_status_code=""201"" home_score=""1"" away_score=""0"" home_games=""0"" away_games=""1""/>
                  <period_score type=""map"" number=""3"" match_status_code=""100"" home_score=""1"" away_score=""0"" home_won_rounds=""13""/>
                </period_scores>
              </sport_event_status>
              <odds/>
            </odds_change>";

        var serializer = new XmlSerializer(typeof(odds_change));
        using var reader = new StringReader(raw);
        var message = (odds_change)serializer.Deserialize(reader);

        var periodScores = message.sport_event_status.period_scores.period_score;
        Assert.Equal(3, periodScores.Length);

        // A completed set keeps its games; the set in progress carries current games.
        Assert.True(periodScores[0].home_gamesSpecified);
        Assert.True(periodScores[0].away_gamesSpecified);
        Assert.Equal(6, periodScores[0].home_games);
        Assert.Equal(4, periodScores[0].away_games);
        Assert.True(periodScores[1].home_gamesSpecified);
        Assert.True(periodScores[1].away_gamesSpecified);
        Assert.Equal(0, periodScores[1].home_games);
        Assert.Equal(1, periodScores[1].away_games);

        // A period without games leaves the Specified flags unset, so the cache
        // maps them to null instead of a false 0:0.
        Assert.False(periodScores[2].home_gamesSpecified);
        Assert.False(periodScores[2].away_gamesSpecified);

        // The sets-won tally on the same row must stay readable and independent.
        Assert.Equal(1, periodScores[0].home_score);
        Assert.Equal(0, periodScores[0].away_score);
    }

    [Fact]
    public void ApiPeriodScoresCarryGames()
    {
        const string raw = @"<?xml version=""1.0""?>
            <match_summary generated_at=""2026-01-01T00:00:00Z"">
              <sport_event id=""od:match:9"" scheduled=""2026-01-01T12:00:00Z""/>
              <sport_event_status status=""live"" match_status_code=""201"" home_score=""1"" away_score=""0"">
                <period_scores>
                  <period_score type=""set"" number=""1"" match_status_code=""200"" home_score=""1"" away_score=""0"" home_games=""6"" away_games=""4""/>
                  <period_score type=""set"" number=""2"" match_status_code=""201"" home_score=""1"" away_score=""0""/>
                </period_scores>
              </sport_event_status>
            </match_summary>";

        var serializer = new XmlSerializer(typeof(MatchSummaryModel));
        using var reader = new StringReader(raw);
        var summary = (MatchSummaryModel)serializer.Deserialize(reader);

        var periodScores = summary.sport_event_status.period_scores;
        Assert.Equal(2, periodScores.Length);

        Assert.True(periodScores[0].home_gamesSpecified);
        Assert.True(periodScores[0].away_gamesSpecified);
        Assert.Equal(6, periodScores[0].home_games);
        Assert.Equal(4, periodScores[0].away_games);

        Assert.False(periodScores[1].home_gamesSpecified);
        Assert.False(periodScores[1].away_gamesSpecified);
    }
}
