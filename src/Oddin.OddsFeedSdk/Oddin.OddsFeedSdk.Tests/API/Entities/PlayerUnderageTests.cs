using System.Globalization;
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
}
