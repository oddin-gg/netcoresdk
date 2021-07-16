using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.Dispatch.EventArguments;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Managers.Recovery
{
    internal class ProducerRecoveryManager : DispatcherBase
    {
        private readonly IProducer _producer;
        private readonly IApiClient _apiClient;
        private readonly IRequestIdFactory _requestIdFactory;

        public event EventHandler<FeedCloseEventArgs> Closed;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        public ProducerRecoveryManager(IProducer producer, IApiClient apiClient, IRequestIdFactory requestIdFactory)
        {
            if (producer is null)
                throw new ArgumentNullException(nameof(producer));

            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (requestIdFactory is null)
                throw new ArgumentNullException(nameof(requestIdFactory));

            _producer = producer;
            _apiClient = apiClient;
            _requestIdFactory = requestIdFactory;
        }

        public bool MatchesProducer(int producerId)
        {
            return _producer.Id == producerId;
        }

        public void Open()
        {
            // TODO: initialization (start timer etc.)
        }

        public void Close()
        {
            // TODO: cleanup (according to Open())
        }

        public void HandleSnapshotCompletedReceived(snapshot_complete message)
        {
            // INFO: SnapshotComplete means the recovery has finished
            // TODO:
            //  - check if request ID matches _producer.RecoveryInfo.RequestId
            //  - mark recovery as completed (set _producer.RecoveryInfo accordingly)
            //  - invoke ProducerUp event (pass _producer to eventArgs)
            //      Dispatch(ProducerUp, ...)
        }

        public void HandleAliveReceived(alive message)
        {
            // INFO: there are 3 possible reasons to request recovery (if it hasn't been already requested), see documentation section 2.4.7
            //  - message.subscribed == 0
            //  - (message.timestamp - previous_message.timestamp) > 10 seconds
            //  - real time between receptions of two consecutive alive messages > 10 seconds

            // TODO:
            //  - check if any of the 3 reasons above occured
            //  - call
            //      await _apiClient.PostRecoveryRequest(producer.Name, _requestIdFactory.GetNext(), nodeId, dateAfter); -- if timestamp of last received message is known (property isn't prepared)
            //      or
            //      await _apiClient.PostRecoveryRequest(producer.Name, _requestIdFactory.GetNext(), nodeId); -- otherwise
            //  - mark recovery as started (set _producer.RecoveryInfo accordingly)
            //  - invoke ProducerDown event (pass _producer to eventArgs
        }
    }
}
