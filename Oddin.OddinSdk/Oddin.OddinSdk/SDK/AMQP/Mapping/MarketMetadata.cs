using Oddin.OddinSdk.SDK.AMQP.Abstractions;
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
            NextBetstop = marketMetadata.next_betstopSpecified
                ? (long?)marketMetadata.next_betstop
                : null;
        }
    }
}
