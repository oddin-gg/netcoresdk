using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IBetCancel<out T> : IMarketMessage<IMarketCancel, T> 
        where T : ISportEvent
    {
        long? StartTime { get; }

        long? EndTime { get; }

        string SupersededBy { get; }
    }
}
