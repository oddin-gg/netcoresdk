using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Mapping;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch.Abstractions;

namespace Oddin.OddinSdk.SDK.Dispatch
{
    internal class ProducerStatusChange : Message, IProducerStatusChange
    {
        public ProducerStatusChange(IProducer producer, IMessageTimestamp messageTimestamp)
            : base(producer, messageTimestamp)
        {
        }
    }
}
