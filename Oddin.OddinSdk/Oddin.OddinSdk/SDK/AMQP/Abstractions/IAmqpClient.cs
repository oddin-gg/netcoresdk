using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.Sessions;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    internal interface IAmqpClient
    {
        /// <summary>
        /// Connects the AMQP consumer to the AMQP broker
        /// </summary>
        void Connect(MessageInterest messageInterest);

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
        event EventHandler<SimpleMessageEventArgs<alive>> AliveMessageReceived;

        /// <summary>
        /// Raised when the AMQP client receives a SnapshotComplete message through AMQP feed
        /// </summary>
        event EventHandler<SimpleMessageEventArgs<snapshot_complete>> SnapshotCompleteMessageReceived;

        /// <summary>
        /// Raised when the AMQP client receives an OddsChange message through AMQP feed
        /// </summary>
        event EventHandler<SimpleMessageEventArgs<odds_change>> OddsChangeMessageReceived;

        /// <summary>
        /// Raised when the AMQP client receives a BetStop message through AMQP feed
        /// </summary>
        event EventHandler<SimpleMessageEventArgs<bet_stop>> BetStopMessageReceived;

        event EventHandler<SimpleMessageEventArgs<bet_settlement>> BetSettlementMessageReceived;
        
        event EventHandler<SimpleMessageEventArgs<bet_cancel>> BetCancelMessageReceived;
    }
}
