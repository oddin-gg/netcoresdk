using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IRollbackBetSettlement<out T> : IMarketMessage<IMarket, T>
        where T : ISportEvent
    {
    }
}
