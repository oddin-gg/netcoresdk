using System;
using Oddin.OddsFeedSdk.AMQP;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.AMQP;

public class TopicParsingHelperTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("hi.pre.-.bet_stop.42.-.123.-", "od:sport:42")]
    [InlineData("hi.pre.-.bet_stop.-.-.123.-", null)]
    public void GetSportUrnParsesSportSection(string routingKey, string expected)
    {
        Assert.Equal(expected, TopicParsingHelper.GetSportURN(routingKey)?.ToString());
    }

    [Fact]
    public void GetSportUrnRejectsWrongSectionCount()
    {
        Assert.Throws<ArgumentException>(() => TopicParsingHelper.GetSportURN("hi.pre.-.bet_stop.42"));
    }

    [Fact]
    public void GetSportUrnRejectsNonNumericSportSection()
    {
        Assert.Throws<ArgumentException>(() => TopicParsingHelper.GetSportURN("hi.pre.-.bet_stop.abc.-.123.-"));
    }
}
