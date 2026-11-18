using System.Globalization;
using System.Reflection;
using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API.Entities;

// clock_running is optional on the wire: an absent attribute is "not said" (null)
// and must stay distinguishable from an explicit false.
[Collection(CacheSideLoadCollection.Name)]
public class ScoreboardClockRunningTests
{
    private static readonly CultureInfo Culture = new("en");

    [Theory]
    [InlineData(" clock_running=\"true\"", true)]
    [InlineData(" clock_running=\"false\"", false)]
    [InlineData("", null)]
    public void FeedScoreboardCarriesClockRunning(string attribute, bool? expected)
    {
        var xml =
            "<odds_change product=\"2\" event_id=\"od:match:1\" timestamp=\"1777832981632\" request_id=\"102\">" +
            "<sport_event_status home_score=\"1\" away_score=\"0\" status=\"1\" scoreboard_available=\"true\" match_status=\"0\">" +
            $"<scoreboard home_goals=\"1\" away_goals=\"0\" time=\"754\" game_time=\"754\"{attribute}/>" +
            "</sport_event_status>" +
            "<odds><market id=\"1\" specifiers=\"variant=way:two|way=two\" status=\"1\">" +
            "<outcome id=\"1\" odds=\"1.85\" active=\"1\"/><outcome id=\"2\" odds=\"1.85\" active=\"1\"/></market></odds>" +
            "</odds_change>";

        Assert.True(FeedMessageDeserializer.TryDeserializeMessage(xml, out var message));
        var oddsChange = Assert.IsType<odds_change>(message);

        var api = DispatchProxy.Create<IApiClient, CacheObserverResilienceTests.PublishOnlyApiClientProxy>();
        using var cache = new MatchStatusCache(api);
        cache.OnFeedMessageReceived(oddsChange);

        var scoreboard = cache.GetMatchStatus(new URN("od:match:1")).Scoreboard;
        Assert.Equal(expected, scoreboard.ClockRunning);
        Assert.Equal(754, scoreboard.GameTime);
    }

    [Theory]
    [InlineData(" clock_running=\"true\"", true)]
    [InlineData(" clock_running=\"false\"", false)]
    [InlineData("", null)]
    public void RestScoreboardCarriesClockRunning(string attribute, bool? expected)
    {
        var xml =
            "<match_summary>" +
            "<sport_event id=\"od:match:1\" name=\"A vs B\"/>" +
            "<sport_event_status status=\"live\" match_status_code=\"1\" scoreboard_available=\"true\">" +
            $"<scoreboard home_goals=\"1\" away_goals=\"0\" time=\"754\" game_time=\"754\"{attribute}/>" +
            "</sport_event_status>" +
            "</match_summary>";

        Assert.True(XmlHelper.TryDeserialize<MatchSummaryModel>(xml, out var summary));

        var api = DispatchProxy.Create<IApiClient, CacheObserverResilienceTests.PublishOnlyApiClientProxy>();
        var proxy = (CacheObserverResilienceTests.PublishOnlyApiClientProxy)api;
        using var cache = new MatchStatusCache(api);
        proxy.Publish(summary, Culture);

        var scoreboard = cache.GetMatchStatus(new URN("od:match:1")).Scoreboard;
        Assert.Equal(expected, scoreboard.ClockRunning);
        Assert.Equal(754, scoreboard.GameTime);
    }
}
