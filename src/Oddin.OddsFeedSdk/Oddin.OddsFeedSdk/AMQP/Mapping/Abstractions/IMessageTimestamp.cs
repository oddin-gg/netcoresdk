namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IMessageTimestamp
    {
        long Created { get; }

        long Sent { get; }

        long Received { get; }

        long Dispatched { get; }
    }
}
