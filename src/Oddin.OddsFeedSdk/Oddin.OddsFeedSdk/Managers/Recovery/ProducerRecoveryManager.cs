using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using System;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.Managers.Recovery
{
    internal class ProducerRecoveryManager : DispatcherBase
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(ProducerRecoveryManager));
        private readonly IFeedConfiguration _config;
        private readonly IProducer _producer;
        private readonly IApiClient _apiClient;
        private readonly IRequestIdFactory _requestIdFactory;
        private bool _isRecoveryInProgress;
        private long _requestId;

        public event EventHandler<FeedCloseEventArgs> Closed;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        public ProducerRecoveryManager(IFeedConfiguration config, IProducer producer, IApiClient apiClient, IRequestIdFactory requestIdFactory)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            if (producer is null)
                throw new ArgumentNullException(nameof(producer));

            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (requestIdFactory is null)
                throw new ArgumentNullException(nameof(requestIdFactory));

            _config = config;
            _producer = producer;
            _apiClient = apiClient;
            _requestIdFactory = requestIdFactory;
            _isRecoveryInProgress = false;
            _requestId = -1;
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

        private ProducerStatusChangeEventArgs CreateProducerStatusChangeEventArgs()
        {
            var messageTimestamp = new MessageTimestamp(DateTime.UtcNow.ToEpochTimeMilliseconds());
            var producerStatusChange = new ProducerStatusChange(_producer, messageTimestamp);
            return new ProducerStatusChangeEventArgs(producerStatusChange);
        }

        // TODO: handle too long recovery (MaxRecoveryExecution) -> recovery error ???
        private async Task StartRecovery()
        {
            ((Producer)_producer).SetProducerDown(true);
            Dispatch(ProducerDown, CreateProducerStatusChangeEventArgs(), nameof(ProducerDown));

            _requestId = _requestIdFactory.GetNext();

            if (_producer.LastTimestampBeforeDisconnect == default)
                await _apiClient.PostRecoveryRequest(_producer.Name, _requestId, _config.NodeId);
            else
                await _apiClient.PostRecoveryRequest(_producer.Name, _requestId, _config.NodeId, _producer.LastTimestampBeforeDisconnect);

            _isRecoveryInProgress = true;
        }

        private void CompleteRecovery()
        {
            ((Producer)_producer).SetProducerDown(false);
            Dispatch(ProducerUp, CreateProducerStatusChangeEventArgs(), nameof(ProducerUp));

            _requestId = -1;
            _isRecoveryInProgress = false;
        }

        public void HandleSnapshotCompletedReceived(snapshot_complete message)
        {
            if (message.request_id != _requestId)
            {
                _log.LogInformation($"A SnapshotComplete message with incorrect request ID was received! Target producer: {_producer.Name}, received request ID: {message.request_id}, current request ID: {_requestId}");
                return;
            }

            CompleteRecovery();
        }

        private void SetLastTimestampBeforeDisconnect(DateTime timestamp)
        {
            ((Producer)_producer).SetLastTimestampBeforeDisconnect(timestamp);
        }

        private bool AreGeneratedTimestampsTooDistant(DateTime receivedTimestamp)
        {
            return receivedTimestamp.Subtract(_producer.LastTimestampBeforeDisconnect) > TimeSpan.FromSeconds(_producer.MaxInactivitySeconds);
        }

        public async Task HandleAliveReceived(alive message)
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

            if (_producer.IsAvailable == false
                || _producer.IsDisabled == true)
            {
                return;
            }

            if ((message.subscribed == 0
                || AreGeneratedTimestampsTooDistant(message.timestamp.FromEpochTimeMilliseconds())
                // ... other recovery reasons
                )
                && _isRecoveryInProgress == false)
            {
                await StartRecovery();
            }

            else if (_isRecoveryInProgress == false)
            {
                var timestamp = message.timestamp.FromEpochTimeMilliseconds();
                SetLastTimestampBeforeDisconnect(timestamp);
            }
        }
    }
}
