using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IOddsChange<out T> : IMarketMessage<IMarketWithOdds, T> 
        where T : ISportEvent
    {
        int? BetStopReason { get; }

        int? BettingStatus { get; }
    }
}
