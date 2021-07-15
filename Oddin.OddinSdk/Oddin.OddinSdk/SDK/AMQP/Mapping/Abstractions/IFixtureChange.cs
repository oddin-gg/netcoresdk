using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IFixtureChange<out T> : IEventMessage<T>
        where T : ISportEvent
    {
        FixtureChangeType? ChangeType { get; }

        long StartTime { get; }

        long? NextLiveTime { get; }
    }
}
