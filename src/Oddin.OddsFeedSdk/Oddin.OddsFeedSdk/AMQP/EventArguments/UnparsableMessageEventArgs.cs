using System;
using System.Text;
using Oddin.OddsFeedSdk.AMQP.Messages;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments;

public class UnparsableMessageEventArgs : EventArgs
{
    private readonly byte[] _rawMessage;

    public UnparsableMessageEventArgs(MessageType messageType, string producer, string eventId, byte[] rawMessage)
    {
        MessageType = messageType;
        Producer = producer;
        EventId = eventId;
        _rawMessage = rawMessage;
    }

    public MessageType MessageType { get; }

    public string Producer { get; }

    public string EventId { get; }

    public string GetRawMessage() =>
        _rawMessage == null
            ? null
            : Encoding.UTF8.GetString(_rawMessage);
}