using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IFixtureChange<out T> : IEventMessage<T>
        where T : ISportEvent
    {
        FixtureChangeType? ChangeType { get; }

        long StartTime { get; }

        long? NextLiveTime { get; }
    }
}
