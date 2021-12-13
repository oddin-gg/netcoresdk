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
        private bool _isRecoveryNeeded;
        private bool _isInitialRecoveryNeeded;
        private bool _isRecoveryInProgress;
        private long _requestId;
        private Timer _aliveMessageReceivedTimer;
        private readonly ElapsedEventHandler _handleProducerDownDelegate;

        private readonly object _lockHandleProducerDown = new();
        private readonly object _lockIsInitialRecoveryNeeded = new();
        private readonly object _lockIsRecoveryNeeded = new();
        private readonly object _lockIsRecoveryInProgress = new();

        public event EventHandler<FeedCloseEventArgs> Closed;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        public ProducerRecoveryManager(IFeedConfiguration config, IProducer producer, IApiClient apiClient, IRequestIdFactory requestIdFactory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _requestIdFactory = requestIdFactory ?? throw new ArgumentNullException(nameof(requestIdFactory));
            _isRecoveryInProgress = false;
            _requestId = -1;
            _isInitialRecoveryNeeded = true;

            _handleProducerDownDelegate = (sender, eventArgs) => HandleProducerDown("Alive message wasn't received in time!");
        }

        public bool MatchesProducer(int producerId)
        {
            return _producer.Id == producerId;
        }

        private void AliveMessageReceivedTimerSetup()
        {
            _aliveMessageReceivedTimer = new Timer(_config.MaxInactivitySeconds * 1000 + TOLERABLE_FEED_COMMUNICATION_DELAY_MILLISECONDS)
            {
                AutoReset = false
            };
            _aliveMessageReceivedTimer.Elapsed += _handleProducerDownDelegate;
        }

        private void AliveMessageReceivedTimerCleanup()
        {
            if (_aliveMessageReceivedTimer is null)
                return;

            _aliveMessageReceivedTimer.Stop();
            _aliveMessageReceivedTimer.Elapsed -= _handleProducerDownDelegate;

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
        }

        public void Close()
        {
            AliveMessageReceivedTimerCleanup();
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
                return;
            }

            SetIsRecoveryInProgress(value: true);
        }

        private async Task StartRecovery()
        {
            if (IgnoreRecovery())
                return;

            if (IsRecoveryInProgress())
            {
                _log.LogWarning("Recovery already in a progress! Skipping.");
                return;
            }

            _log.LogInformation("Starting recovery...");

            _requestId = _requestIdFactory.GetNext();
            await MakeRecoveryRequestToApi();
        }

        private void DispatchProducerDown()
        {
            ((Producer)_producer).SetProducerDown(true);
            Dispatch(ProducerDown, CreateProducerStatusChangeEventArgs(), nameof(ProducerDown));
        }

        private void HandleProducerDown(string reason)
        {
            lock(_lockHandleProducerDown)
            {
                // INFO: if recovery has already been marked as needed (and producer has been marked as down), don't do that again
                if (IsRecoveryNeeded())
                    return;

                _log.LogInformation("Producer down! {reason}", reason);
                DispatchProducerDown();
                SetIsRecoveryNeeded(value: true);
            }
        }

        private void CompleteRecovery()
        {
            _log.LogInformation($"Recovery was completed (request ID: {_requestId})!");

            ((Producer)_producer).SetProducerDown(false);
            Dispatch(ProducerUp, CreateProducerStatusChangeEventArgs(), nameof(ProducerUp));

            _requestId = -1;

            SetIsInitialRecoveryNeeded(false);
            SetIsRecoveryNeeded(value: false);
            SetIsRecoveryInProgress(value: false);
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

        private void SetIsRecoveryNeeded(bool value)
        {
            lock(_lockIsRecoveryNeeded)
            {
                _isRecoveryNeeded = value;
            }
        }

        private bool IsRecoveryNeeded()
        {
            lock(_lockIsRecoveryNeeded)
            {
                return _isRecoveryNeeded;
            }
        }

        private void SetIsInitialRecoveryNeeded(bool value)
        {
            lock(_lockIsInitialRecoveryNeeded)
            {
                _isInitialRecoveryNeeded = value;
            }
        }

        private bool IsInitialRecoveryNeeded()
        {
            lock(_lockIsInitialRecoveryNeeded)
            {
                return _isInitialRecoveryNeeded;
            }
        }

        private void SetIsRecoveryInProgress(bool value)
        {
            lock(_lockIsRecoveryInProgress)
            {
                _isRecoveryInProgress = value;
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
                || _config.IgnoreRecovery == true;

            if (result)
                _log.LogInformation($"Ignoring recovery because: _producer.IsAvailable is {_producer.IsAvailable}(should be false) or _producer.IsDisabled is {_producer.IsDisabled} (should be true) or _config.IgnoreRecovery is {_config.IgnoreRecovery}(should be false)");

            return result;
        }

        public async Task HandleAliveReceived(alive message)
        {
            if (IgnoreRecovery())
               return;

            ResetAliveMessageReceivedTimer();

            if (IsInitialRecoveryNeeded()) {
                if (_config.InitialSnapshotTimeInMinutes != default)
                {
                    var timestampFrom = DateTime.Now.Subtract(new TimeSpan(0, _config.InitialSnapshotTimeInMinutes, 0));
                    SetLastTimestampBeforeDisconnect(timestampFrom);
                }
                HandleProducerDown("Initial recovery is requested!");
            }
            else if (message.subscribed == 0)
            {
                HandleProducerDown("An alive message with subscribed == 0 was received!");
            }
            else if (AreGeneratedTimestampsTooDistant(message.timestamp.FromEpochTimeMilliseconds()))
            {
                HandleProducerDown("An alive message with too old timestamp of generation was received!");
            }
            else if (IsRecoveryInProgress() == false)
            {
                var timestamp = message.timestamp.FromEpochTimeMilliseconds();
                SetLastTimestampBeforeDisconnect(timestamp);
            }

            if (IsRecoveryNeeded())
            {
                // INFO: when alive message is received, communication has probably been restored and we can try starting recovery
                await StartRecovery();
            }
        }
    }
}