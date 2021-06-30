namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    using System;

    public class MessageEventArgs<T> : EventArgs
    {
        public T FeedMessage { get; }

        public byte[] RawMessage { get; }

        public MessageEventArgs(T feedMessage, byte[] rawMessage)
        {
            FeedMessage = feedMessage;
            RawMessage = rawMessage;
        }
    }
}
