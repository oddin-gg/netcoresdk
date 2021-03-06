using System;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class SimpleMessageEventArgs<T> : EventArgs
    {
        public T FeedMessage { get; }

        public byte[] RawMessage { get; }

        public SimpleMessageEventArgs(T feedMessage, byte[] rawMessage)
        {
            FeedMessage = feedMessage;
            RawMessage = rawMessage;
        }
    }
}
