using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IBetCancel<out T> : IMarketMessage<IMarketCancel, T> 
        where T : ISportEvent
    {
        long? StartTime { get; }

        long? EndTime { get; }

        URN SupersededBy { get; }
    }
}
