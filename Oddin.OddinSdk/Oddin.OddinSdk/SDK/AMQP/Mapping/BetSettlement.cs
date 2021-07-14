using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class BetSettlement<T> : MarketMessage<IMarketWithSettlement, T>, IBetSettlement<T> where T : ISportEvent
    {
        public BetSettlementCertainty Certainty { get; }

        public BetSettlement(
            IMessageTimestamp timestamp,
            IProducer producer,
            T @event,
            long? requestId,
            IEnumerable<IMarketWithSettlement> markets,
            int certainty,
            byte[] rawMessage)
            : base(producer, timestamp, @event, requestId, rawMessage, markets)
        {
            Certainty = certainty switch
            {
                1 => BetSettlementCertainty.LiveScouted,
                2 => BetSettlementCertainty.Confirmed,
                _ => BetSettlementCertainty.Unknown
            };
        }
    }
}
