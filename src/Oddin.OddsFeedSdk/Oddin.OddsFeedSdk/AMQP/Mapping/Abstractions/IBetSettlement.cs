using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IBetSettlement<out T> : IMarketMessage<IMarketWithSettlement, T> 
        where T : ISportEvent
    {
        BetSettlementCertainty Certainty { get; }
    }
}
