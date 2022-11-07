using System;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class MarketMetadata : IMarketMetadata
{
    public MarketMetadata(marketMetadata marketMetadata)
    {
        if (marketMetadata is null)
            NextBetstop = null;
        else
            NextBetstop = marketMetadata.next_betstopSpecified
                ? marketMetadata.next_betstop
                : null;
    }

    public long? NextBetstop { get; }

    public DateTime? NextBetstopDate
        => NextBetstop.HasValue
            ? NextBetstop.Value.FromEpochTimeMilliseconds()
            : null;
}