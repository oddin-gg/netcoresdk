using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Tests.API;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API.Entities;

public class PlayerUnderageTests
{
    [Theory]
    [InlineData("1", UnderageStatus.Yes)]
    [InlineData("0", UnderageStatus.No)]
    [InlineData("-1", UnderageStatus.Unknown)]
    [InlineData(null, UnderageStatus.Unknown)]
    [InlineData("", UnderageStatus.Unknown)]
    [InlineData("true", UnderageStatus.Unknown)]
    public void WireEncodingMapsOntoTheEnum(string wire, UnderageStatus expected)
        => Assert.Equal(expected, UnderageStatusParser.Parse(wire));

    [Fact]
    public void LocalizedPlayerDefaultsToUnknown()
        => Assert.Equal(UnderageStatus.Unknown, new LocalizedPlayer(new URN("od:player:1")).Underage);

    [Fact]
    public void PlayerModelDeserializesUnderage()
    {
        const string xml = "<player_profile><player id=\"od:player:1\" name=\"P\" sport=\"od:sport:1\" underage=\"1\"/></player_profile>";
        Assert.True(XmlHelper.TryDeserialize<player_profile>(xml, out var profile));
        Assert.Equal("1", profile.player.underage);
    }

    // Nested players are side-loaded from every competitor profile response, so a
    // later payload (another culture, a nested player) that omits the attribute
    // must keep a known value; only an explicit -1 retracts it.
    [Fact]
    public void CacheCarriesUnderageThroughToThePlayerAndKeepsItWhenAPayloadOmitsIt()
    {
        var api = DispatchProxy.Create<IApiClient, CacheObserverResilienceTests.PublishOnlyApiClientProxy>();
        var proxy = (CacheObserverResilienceTests.PublishOnlyApiClientProxy)api;
        using var cache = new PlayerCache(api);
        var id = new URN("od:player:1");
        var en = new CultureInfo("en");
        var de = new CultureInfo("de");
        var fr = new CultureInfo("fr");

        proxy.Publish(Profile(NestedPlayer("1")), en);
        var player = new Player(id, cache, null, ExceptionHandlingStrategy.CATCH, new[] { en });
        Assert.Equal(UnderageStatus.Yes, player.Underage);

        proxy.Publish(Profile(NestedPlayer(null)), de);
        Assert.Equal(UnderageStatus.Yes, cache.GetPlayer(id, new[] { en, de }).Underage);

        proxy.Publish(Profile(NestedPlayer("-1")), fr);
        Assert.Equal(UnderageStatus.Unknown, cache.GetPlayer(id, new[] { en, de, fr }).Underage);
    }

    private static player_profilePlayer NestedPlayer(string underage) =>
        new() { id = "od:player:1", name = "P", sportID = "od:sport:1", underage = underage };

    private static competitorProfileEndpoint Profile(params player_profilePlayer[] players) =>
        new()
        {
            competitor = new teamExtended { id = "od:competitor:1", name = "Team" },
            players = new List<player_profilePlayer>(players)
        };
}
