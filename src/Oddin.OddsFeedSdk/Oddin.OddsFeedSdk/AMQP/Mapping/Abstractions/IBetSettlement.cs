using System;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IBetSettlement<out T> : IMarketMessage<IMarketWithSettlement, T>
    where T : ISportEvent
{
    [Obsolete("The feed never sends this value.")]
    BetSettlementCertainty Certainty { get; }
}