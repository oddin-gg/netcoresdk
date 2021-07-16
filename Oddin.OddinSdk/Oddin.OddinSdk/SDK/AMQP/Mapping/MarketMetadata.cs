using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class MarketMetadata : IMarketMetadata
    {
        public long? NextBetstop { get; }

        public DateTime? NextBetstopDate
            => NextBetstop.HasValue
                ? (DateTime?)DateTimeOffset.FromUnixTimeMilliseconds(NextBetstop.Value).DateTime
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
