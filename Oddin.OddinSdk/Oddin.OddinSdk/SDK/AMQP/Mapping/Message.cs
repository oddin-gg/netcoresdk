using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
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
