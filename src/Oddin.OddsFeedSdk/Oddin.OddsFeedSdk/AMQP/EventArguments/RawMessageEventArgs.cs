using System;
using System.Text;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Sessions;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class RawMessageEventArgs : EventArgs
    {
        public MessageType MessageType { get; }

        public string Producer { get; }

        public string EventId { get; }

        private readonly byte[] _rawMessage;

        public MessageInterest MessageInterest { get; }

        public string MessageRoutingKey { get; }

        public RawMessageEventArgs(MessageType messageType, MessageInterest messageInterest, string messageRoutingKey,
            string producer, string eventId, byte[] rawMessage)
        {
            MessageType = messageType;
            Producer = producer;
            EventId = eventId;
            MessageInterest = messageInterest;
            MessageRoutingKey = messageRoutingKey;
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