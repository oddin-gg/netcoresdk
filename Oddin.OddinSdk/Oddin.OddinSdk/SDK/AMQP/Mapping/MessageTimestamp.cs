using Oddin.OddinSdk.SDK.AMQP.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
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
    }
}
