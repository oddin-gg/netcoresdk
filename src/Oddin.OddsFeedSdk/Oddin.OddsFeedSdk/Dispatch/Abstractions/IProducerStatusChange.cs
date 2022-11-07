using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.Managers.Recovery;

namespace Oddin.OddsFeedSdk.Dispatch.Abstractions;

public interface IProducerStatusChange : IMessage
{
    bool IsDown { get; }
    bool IsDelayed { get; }
    ProducerStatusReason ProducerStatusReason { get; }
}