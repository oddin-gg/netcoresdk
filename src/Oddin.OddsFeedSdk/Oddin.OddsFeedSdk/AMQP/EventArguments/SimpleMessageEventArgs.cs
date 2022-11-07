using System;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments;

public class SimpleMessageEventArgs<T> : EventArgs
{
    public SimpleMessageEventArgs(T feedMessage, byte[] rawMessage)
    {
        FeedMessage = feedMessage;
        RawMessage = rawMessage;
    }

    public T FeedMessage { get; }

    public byte[] RawMessage { get; }
}