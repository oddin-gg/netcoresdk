using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class BetSettlement<T> : MarketMessage<IMarketWithSettlement, T>, IBetSettlement<T> where T : ISportEvent
{
    public BetSettlement(
        IMessageTimestamp timestamp,
        IProducer producer,
        T @event,
        long? requestId,
        IEnumerable<IMarketWithSettlement> markets,
        byte[] rawMessage)
        : base(producer, timestamp, @event, requestId, rawMessage, markets)
    {
    }

    // The feed never sends a certainty, so it is always unknown.
    public BetSettlementCertainty Certainty => BetSettlementCertainty.Unknown;
}