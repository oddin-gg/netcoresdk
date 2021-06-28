using Oddin.OddinSdk.SDK.AMQP.Messages;
using System;

namespace Oddin.OddinSdk.SDK.AMQP
{
    /// <summary>
    /// An event argument used by events raised when a message from the feed is received
    /// </summary>
    public class FeedMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets deserialized message
        /// </summary>
        public FeedMessage Message { get; }

        // TODO: remove?
        ///// <summary>
        ///// The <see cref="MessageInterest"/> specifying the interest of the associated session
        ///// </summary>
        //public MessageInterest Interest { get; }

        /// <summary>
        /// The raw message
        /// </summary>
        public byte[] RawMessage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedMessageReceivedEventArgs"/> class
        /// </summary>
        /// <param name="message">a <see cref="FeedMessage"/> representing the received message</param>
        /// if no session is associated with the received message</param>
        /// <param name="rawMessage">The raw message</param>
        public FeedMessageReceivedEventArgs(FeedMessage message, byte[] rawMessage)
        {
            Message = message;
            RawMessage = rawMessage;
        }
    }
}
