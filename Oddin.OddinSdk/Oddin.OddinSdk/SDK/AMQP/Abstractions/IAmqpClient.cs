using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.Sessions;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.Abstractions
{
    internal interface IAmqpClient
    {
        void Connect(MessageInterest messageInterest);

        void Disconnect();

        event EventHandler<UnparsableMessageEventArgs> UnparsableMessageReceived;

        event EventHandler<SimpleMessageEventArgs<alive>> AliveMessageReceived;

        /// <summary>
        /// Raised when the AMQP client receives a SnapshotComplete message through AMQP feed
        /// </summary>
        event EventHandler<SimpleMessageEventArgs<snapshot_complete>> SnapshotCompleteMessageReceived;

        /// <summary>
        /// Raised when the AMQP client receives an OddsChange message through AMQP feed
        /// </summary>
        event EventHandler<SimpleMessageEventArgs<odds_change>> OddsChangeMessageReceived;

        event EventHandler<SimpleMessageEventArgs<bet_stop>> BetStopMessageReceived;

        event EventHandler<SimpleMessageEventArgs<bet_settlement>> BetSettlementMessageReceived;
        
        event EventHandler<SimpleMessageEventArgs<bet_cancel>> BetCancelMessageReceived;

        event EventHandler<SimpleMessageEventArgs<fixture_change>> FixtureChangeMessageReceived;
    }
}
