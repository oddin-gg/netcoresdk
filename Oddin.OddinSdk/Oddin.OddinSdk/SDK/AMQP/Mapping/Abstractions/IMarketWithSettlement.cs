using Oddin.OddinSdk.SDK.AMQP.Enums;
using System;
using System.Collections.Generic;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IMarketWithSettlement : IMarketCancel
    {
        MarketStatus MarketStatus { get; }

        IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }
    }
}