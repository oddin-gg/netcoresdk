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
        /// Raised when the AMQP client receives a message through AMQP feed
        /// </summary>
        event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;
    }
}
