using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IMarketWithSettlement : IMarketCancel
{
    MarketStatus MarketStatus { get; }

    IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }
}