using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    public interface IAmqpClient
    {
        /// <summary>
        /// Connects the AMQP consumer to the AMQP broker
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects the AMQP consumer from the AMQP broker
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Raised when the AMQP client receives a message that cannot be parsed through AMQP feed
        /// </summary>
        event EventHandler<UnparsableMessageEventArgs> UnparsableMessageReceived;

        /// <summary>
        /// Raised when the AMQP client receives an Alive message through AMQP feed
        /// </summary>
        event EventHandler<SimpleMessageEventArgs<AliveMessage>> AliveMessageReceived;

        /// <summary>
        /// Raised when the AMQP client receives an OddsChange message throught AMQP feed
        /// </summary>
        event EventHandler<SimpleMessageEventArgs<OddsChangeMessage>> OddsChangeMessageReceived;
    }
}
