using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IBetSettlement<out T> : IMarketMessage<IMarketWithSettlement, T> 
        where T : ISportEvent
    {
        BetSettlementCertainty Certainty { get; }
    }

    public enum BetSettlementCertainty
    {
        LiveScouted = 1,
        Confirmed = 2,
        Unknown = 0
    }
}
