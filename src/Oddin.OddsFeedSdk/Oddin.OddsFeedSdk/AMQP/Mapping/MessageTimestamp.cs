using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class MessageTimestamp : IMessageTimestamp
    {
        public long Created { get; }
        public long Sent { get; }
        public long Received { get; }
        public long Dispatched { get; }

        public MessageTimestamp(long created, long sent, long received, long dispatched)
        {
            Created = created;
            Sent = sent;
            Received = received;
            Dispatched = dispatched;
        }

        public MessageTimestamp(long timestamp)
        {
            Created = timestamp;
            Sent = timestamp;
            Received = timestamp;
            Dispatched = timestamp;
        }
    }
}
