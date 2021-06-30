using Oddin.OddinSdk.SDK.AMQP.Messages;
using System;
using System.Text;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    /// <summary>
    /// Event arguments of <see cref="IOddsFeedSession.OnUnparsableMessageReceived"/> event
    /// </summary>
    public class UnparsableMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="MessageType"/> member specifying the type of the unparsable message
        /// </summary>
        public MessageType MessageType { get; }

        /// <summary>
        /// Gets the <see cref="string"/> representation of the producer associated with the unparsable message
        /// </summary>
        public string Producer { get; }

        /// <summary>
        /// Gets the <see cref="string"/> representation of the sport event id associated with the unparsable message
        /// </summary>
        public string EventId { get; }


        /// <summary>
        /// The raw message
        /// </summary>
        private readonly byte[] _rawMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnparsableMessageEventArgs"/> class
        /// </summary>
        /// <param name="rawMessage">A raw message received from the feed</param>
        /// <param name="messageType">The <see cref="MessageType"/> member specifying the type of the unparsable message.</param>
        /// <param name="producer">The <see cref="string"/> representation of the producer associated with the unparsable message.</param>
        /// <param name="eventId">The <see cref="string"/> representation of the sport event id associated with the unparsable message.</param>
        public UnparsableMessageEventArgs(MessageType messageType/*, string producer, string eventId*/, byte[] rawMessage)
        {
            MessageType = messageType;
            //Producer = producer;
            //EventId = eventId;
            _rawMessage = rawMessage;
        }

        /// <summary>
        /// Gets the raw XML message received from the feed
        /// </summary>
        /// <returns>Returns the raw XML message received from the feed</returns>
        public string GetRawMessage()
        {
            return _rawMessage == null
                ? null
                : Encoding.UTF8.GetString(_rawMessage);
        }
    }
}
