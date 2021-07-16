using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.Abstractions;

namespace Oddin.OddsFeedSdk.Dispatch
{
    internal class ProducerStatusChange : Message, IProducerStatusChange
    {
        public ProducerStatusChange(IProducer producer, IMessageTimestamp messageTimestamp)
            : base(producer, messageTimestamp)
        {
        }
    }
}
