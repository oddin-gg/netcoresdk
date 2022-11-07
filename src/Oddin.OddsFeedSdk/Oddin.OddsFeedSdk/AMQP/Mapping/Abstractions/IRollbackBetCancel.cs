using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IRollbackBetCancel<out T> : IMarketMessage<IMarket, T>
    where T : ISportEvent
{
}