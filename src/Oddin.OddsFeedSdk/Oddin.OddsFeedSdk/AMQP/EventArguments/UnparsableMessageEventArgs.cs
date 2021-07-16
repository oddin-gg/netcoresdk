using Oddin.OddsFeedSdk.AMQP.Messages;
using System;
using System.Text;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class UnparsableMessageEventArgs : EventArgs
    {
        public MessageType MessageType { get; }

        public string Producer { get; }

        public string EventId { get; }

        private readonly byte[] _rawMessage;

        public UnparsableMessageEventArgs(MessageType messageType, string producer, string eventId, byte[] rawMessage)
        {
            MessageType = messageType;
            Producer = producer;
            EventId = eventId;
            _rawMessage = rawMessage;
        }

        public string GetRawMessage()
        {
            return _rawMessage == null
                ? null
                : Encoding.UTF8.GetString(_rawMessage);
        }
    }
}
