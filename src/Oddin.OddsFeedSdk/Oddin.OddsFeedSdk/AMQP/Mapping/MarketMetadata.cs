using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Common;
using System;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class MarketMetadata : IMarketMetadata
    {
        public long? NextBetstop { get; }

        public DateTime? NextBetstopDate
            => NextBetstop.HasValue
                ? (DateTime?)NextBetstop.Value.FromEpochTimeMilliseconds()
                : null;

        public MarketMetadata(marketMetadata marketMetadata)
        {
            if (marketMetadata is null)
                NextBetstop = null;
            else
                NextBetstop = marketMetadata.next_betstopSpecified
                    ? (long?)marketMetadata.next_betstop
                    : null;
        }
    }
}
