using System;
using System.Globalization;
using System.Reflection;
using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.AMQP.Mapping;

public class BetSettlementCertaintyTests
{
    private const string ObsoleteMessage = "The feed never sends this value.";

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    public void CertaintyIsUnknownWhateverTheFeedMessageCarries(string certainty)
    {
        var xml =
            $"<bet_settlement certainty=\"{certainty}\" product=\"1\" event_id=\"od:match:123\" timestamp=\"1\"><outcomes/></bet_settlement>";

        Assert.True(FeedMessageDeserializer.TryDeserializeMessage(xml, out var feedMessage));
        var message = Assert.IsType<bet_settlement>(feedMessage);
        message.RoutingKey = "hi.pre.-.bet_settlement.42.-.123.-";

        var mapper = new FeedMessageMapper(new FeedMessageMapperTests.StubProducerManager(),
            new FeedMessageMapperTests.StubFeedConfiguration(), null,
            new FeedMessageMapperTests.CapturingSportDataBuilder());

        var mapped = mapper.MapBetSettlement<IMatch>(message, Array.Empty<CultureInfo>(), Array.Empty<byte>());

#pragma warning disable CS0618
        Assert.Equal(BetSettlementCertainty.Unknown, mapped.Certainty);
#pragma warning restore CS0618
    }

    [Fact]
    public void CertaintyIsObsoleteAsWarningOnTheInterfaceAndTheRawModel()
    {
#pragma warning disable CS0618
        AssertObsolete(typeof(IBetSettlement<>).GetProperty(nameof(IBetSettlement<ISportEvent>.Certainty)));
        AssertObsolete(typeof(bet_settlement).GetProperty(nameof(bet_settlement.certainty)));
#pragma warning restore CS0618
    }

    [Fact]
    public void TheEnumIsNotObsolete() =>
        Assert.Null(typeof(BetSettlementCertainty).GetCustomAttribute<ObsoleteAttribute>());

    private static void AssertObsolete(PropertyInfo property)
    {
        Assert.NotNull(property);
        var attribute = property.GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(attribute);
        Assert.False(attribute.IsError);
        Assert.Equal(ObsoleteMessage, attribute.Message);
    }
}
