using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Dispatch.Abstractions;
using Oddin.OddsFeedSdk.Managers.Recovery;

namespace Oddin.OddsFeedSdk.Dispatch;

internal class ProducerStatusChange : Message, IProducerStatusChange
{
    public ProducerStatusChange(
        IProducer producer,
        IMessageTimestamp messageTimestamp,
        bool isDown,
        bool isDelayed,
        ProducerStatusReason producerStatusReason) : base(producer, messageTimestamp)
    {
        IsDown = isDown;
        IsDelayed = isDelayed;
        ProducerStatusReason = producerStatusReason;
    }

    public bool IsDown { get; }
    public bool IsDelayed { get; }
    public ProducerStatusReason ProducerStatusReason { get; }
}