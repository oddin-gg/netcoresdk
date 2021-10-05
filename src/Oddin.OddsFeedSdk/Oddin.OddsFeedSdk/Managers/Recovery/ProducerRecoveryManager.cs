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
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Oddin.OddsFeedSdk.Managers.Recovery
{
    internal class ProducerRecoveryManager : DispatcherBase
    {
        private const int TOLERABLE_API_COMMUNICATION_DELAY_SECONDS = 5;
        private const int TOLERABLE_FEED_COMMUNICATION_DELAY_MILLISECONDS = 50;
        private const int TOLERABLE_MESSAGE_TIMESTAMP_DELAY_MILLISECONDS = 10;

        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(ProducerRecoveryManager));
        private readonly IFeedConfiguration _config;
        private readonly IProducer _producer;
        private readonly IApiClient _apiClient;
        private readonly IRequestIdFactory _requestIdFactory;
        private readonly object _lockIsRecoveryInProgress = new();
        private bool _isRecoveryInProgress;
        private long _requestId;
        private Timer _recoveryRequestTimer;
        private readonly ElapsedEventHandler _recoveryRequestDelegate;
        private Timer _aliveMessageReceivedTimer;
        private readonly ElapsedEventHandler _startRecoveryDelegate;

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
            _startRecoveryDelegate = async (sender, eventArgs) => await StartRecoveryBasedOnReceptionTime();
        }
        
        public bool MatchesProducer(int producerId)
        {
            return _producer.Id == producerId;
        }

        private void RecoveryRequestTimerSetup()
        {
            // INFO: _config.MaxRecoveryTime is maximum recovery execution time in seconds
            _recoveryRequestTimer = new Timer(_config.MaxRecoveryTime * 1000)
            {
                AutoReset = true
            };
            _recoveryRequestTimer.Elapsed += _recoveryRequestDelegate;
        }

        private void RecoveryRequestTimerCleanup()
        {
            if (_recoveryRequestTimer is null)
                return;

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

        private void AliveMessageReceivedTimerSetup()
        {
            _aliveMessageReceivedTimer = new Timer(_config.MaxInactivitySeconds * 1000 + TOLERABLE_FEED_COMMUNICATION_DELAY_MILLISECONDS)
            {
                AutoReset = false
            };
            _aliveMessageReceivedTimer.Elapsed += _startRecoveryDelegate;
        }

        private void AliveMessageReceivedTimerCleanup()
        {
            if (_aliveMessageReceivedTimer is null)
                return;

            _aliveMessageReceivedTimer.Stop();
            _aliveMessageReceivedTimer.Elapsed -= _startRecoveryDelegate;

            try
            {
                _aliveMessageReceivedTimer.Dispose();
            }
            catch (Exception)
            {
                _log.LogWarning($"An exception was thrown when disposing {nameof(_aliveMessageReceivedTimer)} on {typeof(ProducerRecoveryManager).Name}!");
            }
        }

        private void ResetAliveMessageReceivedTimer()
        {
            if (IgnoreRecovery())
                return;

            _aliveMessageReceivedTimer.Stop();
            _aliveMessageReceivedTimer.Start();
        }

        public void Open()
        {
            if (IgnoreRecovery())
                return;

            AliveMessageReceivedTimerSetup();
            _aliveMessageReceivedTimer.Start();

            RecoveryRequestTimerSetup();
        }

        public void Close()
        {
            AliveMessageReceivedTimerCleanup();

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
            var tolerableCommunicationDelay = TimeSpan.FromSeconds(TOLERABLE_API_COMMUNICATION_DELAY_SECONDS);

            var maxRecoveryTime = TimeSpan.FromMinutes(_producer.MaxRecoveryTime);
            var oldestFeasibleTimestamp = DateTime.UtcNow.Subtract(maxRecoveryTime).Add(tolerableCommunicationDelay);
            return lastTimestampBeforeDisconnect > oldestFeasibleTimestamp
                ? lastTimestampBeforeDisconnect
                : oldestFeasibleTimestamp;
        }

        private async Task MakeRecoveryRequestToApi()
        {
            try
            {
                if (_producer.LastTimestampBeforeDisconnect == default)
                {
                    _log.LogInformation($"Sending recovery request (producer name: {_producer.Name}, request ID: {_requestId}, node ID: {_config.NodeId})");
                    await _apiClient.PostRecoveryRequest(_producer.Name, _requestId, _config.NodeId);
                }
                else
                {
                    var timestampFrom = GetRecoveryTimestamp(_producer.LastTimestampBeforeDisconnect);
                    _log.LogInformation($"Sending recovery request (producer name: {_producer.Name}, request ID: {_requestId}, node ID: {_config.NodeId}, UTC timestamp: {timestampFrom})");
                    await _apiClient.PostRecoveryRequest(_producer.Name, _requestId, _config.NodeId, timestampFrom);
                }
            }
            catch (CommunicationException e)
            {
                _log.LogError($"Recovery request to API failed!", e);
            }
        }

        private async Task StartRecovery()
        {
            if (IgnoreRecovery()
                || TrySetIsRecoveryInProgress() == false)
                return;

            _log.LogInformation("Starting recovery...");

            ((Producer)_producer).SetProducerDown(true);
            Dispatch(ProducerDown, CreateProducerStatusChangeEventArgs(), nameof(ProducerDown));

            _requestId = _requestIdFactory.GetNext();

            await MakeRecoveryRequestToApi();
            _recoveryRequestTimer.Start();
        }

        private async Task StartRecoveryBasedOnReceptionTime()
        {
            _log.LogInformation("Alive message wasn't received in time!");
            await StartRecovery();
        }

        private void CompleteRecovery()
        {
            _recoveryRequestTimer.Stop();

            _log.LogInformation($"Recovery was completed (request ID: {_requestId})!");

            ((Producer)_producer).SetProducerDown(false);
            Dispatch(ProducerUp, CreateProducerStatusChangeEventArgs(), nameof(ProducerUp));

            _requestId = -1;

            ResetIsRecoveryInProgress();
        }

        public void HandleSnapshotCompletedReceived(snapshot_complete message)
        {
            if (message.request_id != _requestId)
            {
                // INFO: this is a valid possibility since EventRecoveryIssuer is public and allows issuing recovery requests from outside of the Sdk
                return;
            }

            var timestamp = message.timestamp.FromEpochTimeMilliseconds();
            SetLastTimestampBeforeDisconnect(timestamp);

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
            var tolerableDelay = TimeSpan.FromMilliseconds(TOLERABLE_MESSAGE_TIMESTAMP_DELAY_MILLISECONDS);

            return inactivityPeriod > (maxInactivityPeriod + tolerableDelay);
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
                {
                    _log.LogWarning("Recovery already in a progress! Skipping.");
                    return false;
                }

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

        private bool IgnoreRecovery()
        {
            var result = _producer.IsAvailable == false
                || _producer.IsDisabled == true
                || _config.Environment == SdkEnvironment.Replay;

            if (result)
                _log.LogInformation($"Ingoring recovery because: _producer.IsAvailable is {_producer.IsAvailable}(should be false) or _producer.IsDisabled is {_producer.IsDisabled} (should be true) or _config.Environment is {_config.Environment}(should be Replay)");

            return result;
        }

        public async Task HandleAliveReceived(alive message)
        {
            if (IgnoreRecovery())
               return;

            ResetAliveMessageReceivedTimer();

            if (message.subscribed == 0)
            {
                _log.LogInformation("An alive message with subscribed == 0 was received!");
                await StartRecovery();
            }
            else if (AreGeneratedTimestampsTooDistant(message.timestamp.FromEpochTimeMilliseconds()))
            {
                _log.LogInformation("An alive message with too old timestamp of generation was received!");
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
