using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IEventMessage<out T> : IMessage 
        where T : ISportEvent
    {
        T Event { get; }

        long? RequestId { get; }

        byte[] RawMessage { get; }
    }
}
