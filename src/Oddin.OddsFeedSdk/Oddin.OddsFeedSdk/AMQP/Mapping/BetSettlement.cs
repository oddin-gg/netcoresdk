﻿using System.Collections.Generic;
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
        int certainty,
        byte[] rawMessage)
        : base(producer, timestamp, @event, requestId, rawMessage, markets) =>
        Certainty = certainty switch
        {
            1 => BetSettlementCertainty.LiveScouted,
            2 => BetSettlementCertainty.Confirmed,
            _ => BetSettlementCertainty.Unknown
        };

    public BetSettlementCertainty Certainty { get; }
}