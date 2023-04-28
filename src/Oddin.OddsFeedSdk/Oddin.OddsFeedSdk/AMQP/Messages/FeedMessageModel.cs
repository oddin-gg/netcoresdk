using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.AMQP.Messages
{
    public abstract class FeedMessageModel
    {
        public abstract long GeneratedAt { get; }
        public long SentAt { get; set; }
        public long ReceivedAt { get; set; }
        public long DispatchedAt { get; set; }
        public string RoutingKey { get; set; }
        public abstract int ProducerId { get; }


        public MessageType GetMessageType()
        {
            return TopicParsingHelper.GetMessageType(RoutingKey);
        }

        public URN GetSportURNFromRoutingKey()
        {
            return TopicParsingHelper.GetSportURN(RoutingKey);
        }

    }
}
