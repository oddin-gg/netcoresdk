using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Sessions;
using System;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Abstractions
{
    internal interface IAmqpClient
    {
        void Connect(MessageInterest messageInterest, IEnumerable<string> routingKeys);
        void Disconnect();
        event EventHandler<UnparsableMessageEventArgs> UnparsableMessageReceived;
        event EventHandler<SimpleMessageEventArgs<alive>> AliveMessageReceived;
        event EventHandler<SimpleMessageEventArgs<snapshot_complete>> SnapshotCompleteMessageReceived;
        event EventHandler<SimpleMessageEventArgs<odds_change>> OddsChangeMessageReceived;
        event EventHandler<SimpleMessageEventArgs<bet_stop>> BetStopMessageReceived;
        event EventHandler<SimpleMessageEventArgs<bet_settlement>> BetSettlementMessageReceived;
        event EventHandler<SimpleMessageEventArgs<bet_cancel>> BetCancelMessageReceived;
        event EventHandler<SimpleMessageEventArgs<fixture_change>> FixtureChangeMessageReceived;
        public event EventHandler<SimpleMessageEventArgs<MessageProcessingEventArgs>> MessageProcessingStarted;
        public event EventHandler<SimpleMessageEventArgs<MessageProcessingEventArgs>> MessageProcessingEnded;
    }
}