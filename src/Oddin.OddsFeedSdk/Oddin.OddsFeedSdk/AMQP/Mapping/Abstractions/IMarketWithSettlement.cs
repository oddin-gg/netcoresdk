using Oddin.OddsFeedSdk.AMQP.Enums;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IMarketWithSettlement : IMarketCancel
    {
        MarketStatus MarketStatus { get; }

        IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }
    }
}