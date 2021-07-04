namespace Oddin.OddinSdk.SDK.AMQP.Messages
{
    public abstract class FeedMessageModel
    {
        public abstract long GeneratedAt { get; }
        public long SentAt { get; set; }
        public long ReceivedAt { get; set; }
        public long DispatchedAt { get; set; }
    }
}
