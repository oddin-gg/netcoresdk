using System;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class MessageProcessingEventArgs : EventArgs
    {
        public readonly int ProducerId;
        public readonly long?Timestamp;
        public Guid SessionId;

        public MessageProcessingEventArgs(int producerId, long? timestamp)
        {
            ProducerId = producerId;
            Timestamp = timestamp;
        }

        public MessageProcessingEventArgs(int producerId, long? timestamp, Guid sessionId)
        {
            ProducerId = producerId;
            Timestamp = timestamp;
            SessionId = sessionId;
        }
    }
}