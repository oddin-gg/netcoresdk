using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IMessage
    {
        IProducer Producer { get; }

        IMessageTimestamp Timestamps { get; }
    }
}
