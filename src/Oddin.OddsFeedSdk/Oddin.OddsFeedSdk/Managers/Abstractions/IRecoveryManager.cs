using System;
using System.Net;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;

namespace Oddin.OddsFeedSdk.Managers.Abstractions
{
    public interface IRecoveryManager
    {
        HttpStatusCode InitiateEventOddsMessagesRecovery(IProducer producer, URN eventId);
        HttpStatusCode InitiateEventStatefulMessagesRecovery(IProducer producer, URN eventId);
    }

    internal interface ISdkRecoveryManager: IRecoveryManager
    {
        void Open(bool replayOnly);

        void OnMessageProcessingStarted(Guid sessionId, int producerId, long timestamp);
        public void OnMessageProcessingEnded(Guid sessionId, int producerId, long? timestamp);
        void OnAliveReceived(object sender, AliveEventArgs eventArgs);
        void OnSnapshotCompleteReceived(object sender, SnapshotCompleteEventArgs eventArgs);

        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
        public event EventHandler<ProducerStatusChangeEventArgs> EventProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> EventProducerUp;
    }
}