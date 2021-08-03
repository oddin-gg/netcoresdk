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
using System.Timers;

namespace Oddin.OddsFeedSdk.Managers.Recovery
{
    internal class ProducerRecoveryManager : DispatcherBase
    {
        private const int COMMUNICATION_DELAY_SECONDS = 10;

        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(ProducerRecoveryManager));
        private readonly IFeedConfiguration _config;
        private readonly IProducer _producer;
        private readonly IApiClient _apiClient;
        private readonly IRequestIdFactory _requestIdFactory;
        private readonly object _lockIsRecoveryInProgress = new object();
        private bool _isRecoveryInProgress;
        private long _requestId;
        private Timer _recoveryRequestTimer;
        private ElapsedEventHandler _recoveryRequestDelegate;

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

            _recoveryRequestDelegate = async (sender, eventArgs) => await MakeRecoveryRequestToApi();
        }
        
        public bool MatchesProducer(int producerId)
        {
            return _producer.Id == producerId;
        }

        private void RecoveryRequestTimerSetup()
        {
            // INFO: _config.MaxRecoveryTime is maximum recovery execution time in seconds
            _recoveryRequestTimer = new Timer(_config.MaxRecoveryTime * 1000);
            _recoveryRequestTimer.Elapsed += _recoveryRequestDelegate;
        }

        private void RecoveryRequestTimerCleanup()
        {
            _recoveryRequestTimer.Stop();
            _recoveryRequestTimer.Elapsed -= _recoveryRequestDelegate;

            try
            {
                _recoveryRequestTimer.Dispose();
            }
            catch (Exception)
            {
                _log.LogWarning($"An exception was thrown when disposing {nameof(_recoveryRequestTimer)} on {typeof(ProducerRecoveryManager).Name}!");
            }
        }

        public void Open()
        {
            // TODO: initialization (start timer etc.)

            RecoveryRequestTimerSetup();
        }

        public void Close()
        {
            // TODO: cleanup (according to Open())

            RecoveryRequestTimerCleanup();
        }

        private ProducerStatusChangeEventArgs CreateProducerStatusChangeEventArgs()
        {
            var messageTimestamp = new MessageTimestamp(DateTime.UtcNow.ToEpochTimeMilliseconds());
            var producerStatusChange = new ProducerStatusChange(_producer, messageTimestamp);
            return new ProducerStatusChangeEventArgs(producerStatusChange);
        }

        private DateTime GetRecoveryTimestamp(DateTime lastTimestampBeforeDisconnect)
        {
            // INFO: API would return an error if the timestamp was older than MaxRecoveryTime
            var communicationDelay = TimeSpan.FromSeconds(COMMUNICATION_DELAY_SECONDS);

            var maxRecoveryTime = TimeSpan.FromMinutes(_producer.MaxRecoveryTime);
            var oldestFeasibleTimestamp = DateTime.UtcNow.Subtract(maxRecoveryTime).Add(communicationDelay);
            return lastTimestampBeforeDisconnect > oldestFeasibleTimestamp
                ? lastTimestampBeforeDisconnect
                : oldestFeasibleTimestamp;
        }

        private async Task MakeRecoveryRequestToApi()
        {
            if (_producer.LastTimestampBeforeDisconnect == default)
                await _apiClient.PostRecoveryRequest(_producer.Name, _requestId, _config.NodeId);
            else
            {
                var timestampFrom = GetRecoveryTimestamp(_producer.LastTimestampBeforeDisconnect);
                await _apiClient.PostRecoveryRequest(_producer.Name, _requestId, _config.NodeId, timestampFrom);
            }
        }

        private async Task StartRecovery()
        {
            if (TrySetIsRecoveryInProgress() == false)
                return;

            ((Producer)_producer).SetProducerDown(true);
            Dispatch(ProducerDown, CreateProducerStatusChangeEventArgs(), nameof(ProducerDown));

            _requestId = _requestIdFactory.GetNext();

            await MakeRecoveryRequestToApi();
            _recoveryRequestTimer.Start();
        }

        private void CompleteRecovery()
        {
            _recoveryRequestTimer.Stop();

            ((Producer)_producer).SetProducerDown(false);
            Dispatch(ProducerUp, CreateProducerStatusChangeEventArgs(), nameof(ProducerUp));

            _requestId = -1;

            ResetIsRecoveryInProgress();
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
            if (_producer.LastTimestampBeforeDisconnect == default)
                return false;

            var maxInactivityPeriod = TimeSpan.FromSeconds(_producer.MaxInactivitySeconds);
            var inactivityPeriod = receivedTimestamp.Subtract(_producer.LastTimestampBeforeDisconnect);
            return inactivityPeriod > maxInactivityPeriod;
        }

        /// <summary>
        /// Checks the value of <see cref="_isRecoveryInProgress"/> and sets it to <see langword="true"/> if not already set
        /// </summary>
        /// <returns><see langword="false"/> if <see cref="_isRecoveryInProgress"/> is already set to <see langword="true"/>; otherwise returns <see langword="true"/></returns>
        private bool TrySetIsRecoveryInProgress()
        {
            lock(_lockIsRecoveryInProgress)
            {
                if (_isRecoveryInProgress)
                    return false;

                _isRecoveryInProgress = true;
                return true;
            }
        }

        private void ResetIsRecoveryInProgress()
        {
            lock(_lockIsRecoveryInProgress)
            {
                _isRecoveryInProgress = false;
            }    
        }

        private bool IsRecoveryInProgress()
        {
            lock(_lockIsRecoveryInProgress)
            {
                return _isRecoveryInProgress;
            }
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

            if (message.subscribed == 0
                || AreGeneratedTimestampsTooDistant(message.timestamp.FromEpochTimeMilliseconds())
                )
            {
                await StartRecovery();
            }

            else if (IsRecoveryInProgress() == false)
            {
                var timestamp = message.timestamp.FromEpochTimeMilliseconds();
                SetLastTimestampBeforeDisconnect(timestamp);
            }
        }
    }
}
