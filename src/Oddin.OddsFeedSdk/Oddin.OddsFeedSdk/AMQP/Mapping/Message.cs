using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal abstract class Message : IMessage
    {
        public IProducer Producer { get; }

        public IMessageTimestamp Timestamps { get; }

        protected Message(IProducer producer, IMessageTimestamp messageTimestamp)
        {
            Producer = producer;
            Timestamps = messageTimestamp;
        }
    }
}
