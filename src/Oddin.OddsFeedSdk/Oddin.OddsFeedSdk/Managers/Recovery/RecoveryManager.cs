using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Sessions;

namespace Oddin.OddsFeedSdk.Managers.Recovery;

internal class RecoveryManager : DispatcherBase, ISdkRecoveryManager
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(RecoveryManager));
    private readonly IApiClient _apiClient;
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<Guid, long> _messageProcessingTimes;
    private readonly IFeedConfiguration _oddsFeedConfiguration;
    private readonly IProducerManager _producerManager;

    private readonly ConcurrentDictionary<int, ProducerRecoveryData> _producerRecoveryData;
    private readonly IRequestIdFactory _requestIdFactory;
    private bool _isOpened;
    private Timer _timer;

    public RecoveryManager(
        IFeedConfiguration oddsFeedConfiguration,
        IProducerManager producerManager,
        IApiClient apiClient,
        IRequestIdFactory requestIdFactory
    )
    {
        _oddsFeedConfiguration =
            oddsFeedConfiguration ?? throw new ArgumentNullException(nameof(oddsFeedConfiguration));
        _producerManager = producerManager ?? throw new ArgumentNullException(nameof(producerManager));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _requestIdFactory = requestIdFactory ?? throw new ArgumentNullException(nameof(requestIdFactory));

        _producerRecoveryData = new ConcurrentDictionary<int, ProducerRecoveryData>();
        _messageProcessingTimes = new ConcurrentDictionary<Guid, long>();
    }

    public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
    public event EventHandler<ProducerStatusChangeEventArgs> EventProducerDown;
    public event EventHandler<ProducerStatusChangeEventArgs> EventProducerUp;

    public void Open(bool replayOnly)
    {
        lock (_lock)
        {
            if (replayOnly)
            {
                return;
            }

            if (_isOpened)
            {
                _log.LogWarning("Recovery manager already opened. Skipping");
                return;
            }

            var activeProducers = _producerManager.Producers;
            if (!activeProducers.Any())
            {
                _log.LogWarning("No active producers");
            }

            foreach (var activeProducer in activeProducers)
            {
                var producerRecoveryData = new ProducerRecoveryData(activeProducer.Id, _producerManager);
                _producerRecoveryData.AddOrUpdate(activeProducer.Id, producerRecoveryData,
                    (_, _) => producerRecoveryData);
            }

            _timer = new Timer(TimerTick,
                null,
                TimeSpan.FromSeconds(60),
                TimeSpan.FromSeconds(10)
            );

            _isOpened = true;
        }
    }

    public void OnMessageProcessingStarted(Guid sessionId, int producerId, long timestamp)
    {
        lock (_lock)
        {
            _messageProcessingTimes.AddOrUpdate(sessionId, timestamp, (_, _) => timestamp);
            FindOrMakeProducerRecoveryData(producerId).LastMessageReceivedTimestamp = timestamp;
        }
    }

    public void OnMessageProcessingEnded(Guid sessionId, int producerId, long? timestamp)
    {
        lock (_lock)
        {
            if (timestamp != null)
            {
                FindOrMakeProducerRecoveryData(producerId).LastProcessedMessageGenTimestamp = (long)timestamp;
            }

            var started = _messageProcessingTimes.TryGetValue(sessionId, out var start);
            if (started == false || start == 0)
            {
                _log.LogDebug("Message processing ended, but was not started");
            }
            else
            {
                var processTime = Timestamp.Now() - start;
                if (processTime > 1000L)
                {
                    _log.LogWarning("Processing message took more than 1s");
                }

                _messageProcessingTimes.AddOrUpdate(sessionId, 0, (_, _) => 0);
            }
        }
    }

    public void OnAliveReceived(object sender, AliveEventArgs eventArgs)
    {
        var producerId = eventArgs.ProducerId;
        var timestamp = eventArgs.MessageTimestamp;
        var messageInterest = eventArgs.MessageInterest;
        var isSubscribed = eventArgs.IsSubscribed;

        lock (_lock)
        {
            var producerRecoveryData = FindOrMakeProducerRecoveryData(producerId);

            if (producerRecoveryData.IsDisabled)
            {
                return;
            }

            if (messageInterest.MessageInterestType == MessageInterest.SystemAliveOnlyMessages.MessageInterestType)
            {
                SystemSessionAliveReceived(timestamp, isSubscribed, producerRecoveryData);
            }
            else
            {
                producerRecoveryData.LastUserSessionAliveReceivedTimestamp = timestamp.Created;
            }
        }
    }

    public void OnSnapshotCompleteReceived(object sender, SnapshotCompleteEventArgs eventArgs)
    {
        var producerId = eventArgs.ProducerId;
        var requestId = eventArgs.RequestId;
        var messageInterest = eventArgs.MessageInterest;

        lock (_lock)
        {
            if (!_producerRecoveryData.TryGetValue(producerId, out var producerRecoveryData))
            {
                return;
            }

            if (producerRecoveryData.IsDisabled)
            {
                _log.LogInformation($"Received snapshot recovery complete for disabled producer {producerId}");
            }
            else if (!producerRecoveryData.IsKnownRecovery(requestId))
            {
                _log.LogInformation(
                    $"Unknown snapshot recovery complete received for request {requestId} and producer {producerId}"
                );
            }
            else if (producerRecoveryData.ValidateSnapshotComplete(requestId, messageInterest))
            {
                SnapshotRecoveryFinished(
                    requestId,
                    producerRecoveryData
                );
            }
            else if (producerRecoveryData.ValidateEventSnapshotComplete(requestId, messageInterest))
            {
                EventRecoveryFinished(
                    requestId,
                    producerRecoveryData
                );
            }
        }
    }

    public HttpStatusCode InitiateEventOddsMessagesRecovery(IProducer producer, URN eventId) =>
        MakeEventRecovery(producer, eventId, _apiClient.PostEventRecoveryRequest);

    public HttpStatusCode InitiateEventStatefulMessagesRecovery(IProducer producer, URN eventId) =>
        MakeEventRecovery(producer, eventId, _apiClient.PostEventStatefulRecoveryRequest);

    public void Close()
    {
        _isOpened = false;
        _timer.Dispose();
        _timer = null;
    }

    private void SnapshotRecoveryFinished(long requestId, ProducerRecoveryData producerRecoveryData)
    {
        var started = producerRecoveryData.LastRecoveryStartedAt ??
                      throw new SdkException("Inconsistent recovery state");

        var finished = Timestamp.Now();
        _log.LogInformation($"Recovery finished for request {requestId} in {finished - started} ms");

        if (producerRecoveryData.RecoveryState == RecoveryState.Interrupted)
        {
            MakeSnapshotRecovery(
                producerRecoveryData,
                producerRecoveryData.LastValidAliveGenTimestampInRecovery
            );
            return;
        }

        var reason = producerRecoveryData.FirstRecoveryCompleted
            ? ProducerUpReason.ReturnedFromInactivity
            : ProducerUpReason.FirstRecoveryCompleted;

        if (!producerRecoveryData.FirstRecoveryCompleted)
        {
            producerRecoveryData.FirstRecoveryCompleted = true;
        }

        producerRecoveryData.SetProducerRecoveryState(requestId, started, RecoveryState.Completed);
        ProducerUp(producerRecoveryData, reason);
    }


    private void EventRecoveryFinished(long requestId, ProducerRecoveryData producerRecoveryData)
    {
        var eventRecovery = producerRecoveryData.GetEventRecovery(requestId) ??
                            throw new SdkException("Inconsistent event recovery state");

        var started = eventRecovery.RecoveryStartedAt;
        var finished = Timestamp.Now();
        _log.LogInformation(
            $"Event ${eventRecovery.EventId} recovery finished for request {requestId} in {finished - started} ms"
        );

        var eventArgs = new EventRecoveryCompletedEventArgs(requestId, eventRecovery.EventId);
        Dispatch(EventRecoveryCompleted, eventArgs, nameof(EventRecoveryCompleted));

        producerRecoveryData.EventRecoveryCompleted(requestId);
    }


    private HttpStatusCode MakeEventRecovery(IProducer producer, URN eventId,
        Func<string, URN, long, int?, Task<HttpStatusCode>> apiCall)
    {
        lock (_lock)
        {
            var now = Timestamp.Now();
            var producerRecoveryData = FindOrMakeProducerRecoveryData(producer.Id);
            var producerName = producer.Name;
            var requestId = _requestIdFactory.GetNext();
            producerRecoveryData.SetEventRecoveryState(eventId, requestId, now);

            HttpStatusCode statusCode;

            try
            {
                var nodeId = _oddsFeedConfiguration.NodeId;
                _log.LogInformation(
                    $"Sending recovery request (producer name: {producerName}, request ID: {requestId}, node ID: {nodeId})");
                var task = apiCall(producerName, eventId, requestId, nodeId);
                task.Wait();
                statusCode = task.Result;
            }
            catch (SdkException e)
            {
                _log.LogError($"Recovery request to API failed: {e}");
                e.HandleAccordingToStrategy(GetType().Name, _log, _oddsFeedConfiguration.ExceptionHandlingStrategy);
                statusCode = HttpStatusCode.InternalServerError;
            }

            if (!statusCode.IsSuccessStatusCode())
            {
                producerRecoveryData.EventRecoveryCompleted(requestId);
            }

            return statusCode;
        }
    }

    private void SystemSessionAliveReceived(
        IMessageTimestamp timestamp,
        bool subscribed,
        ProducerRecoveryData producerRecoveryData)
    {
        producerRecoveryData.LastMessageReceivedTimestamp = timestamp.Received;
        if (!subscribed)
        {
            if (!producerRecoveryData.IsFlaggedDown)
            {
                ProducerDown(producerRecoveryData, ProducerDownReason.Other);
            }

            MakeSnapshotRecovery(producerRecoveryData, producerRecoveryData.TimestampForRecovery);
            return;
        }

        var now = Timestamp.Now();
        var isBackFromInactivity = producerRecoveryData.IsFlaggedDown &&
                                   !producerRecoveryData.IsPerformingRecovery &&
                                   producerRecoveryData.ProducerDownReason ==
                                   ProducerDownReason.ProcessingQueueDelayViolation &&
                                   CalculateTiming(producerRecoveryData, now);

        var isInRecovery = producerRecoveryData.RecoveryState != RecoveryState.NotStarted &&
                           producerRecoveryData.RecoveryState != RecoveryState.Error &&
                           producerRecoveryData.RecoveryState != RecoveryState.Interrupted;

        if (isBackFromInactivity)
        {
            ProducerUp(producerRecoveryData, ProducerUpReason.ReturnedFromInactivity);
        }
        else if (isInRecovery)
        {
            if (producerRecoveryData.IsFlaggedDown &&
                !producerRecoveryData.IsPerformingRecovery &&
                producerRecoveryData.ProducerDownReason != ProducerDownReason.ProcessingQueueDelayViolation)
            {
                MakeSnapshotRecovery(producerRecoveryData, producerRecoveryData.TimestampForRecovery);
            }

            var recoveryTiming = now - ( producerRecoveryData.LastRecoveryStartedAt ?? 0 );
            var maxInterval = Timestamp.FromMinutes(_oddsFeedConfiguration.MaxRecoveryExecutionMinutes);
            if (producerRecoveryData.IsPerformingRecovery && recoveryTiming > maxInterval)
            {
                // @TODO recoveryId 0
                producerRecoveryData.SetProducerRecoveryState(0, 0, RecoveryState.Error);
                MakeSnapshotRecovery(producerRecoveryData, producerRecoveryData.TimestampForRecovery);
            }
        }
        else
        {
            MakeSnapshotRecovery(producerRecoveryData, producerRecoveryData.TimestampForRecovery);
        }

        producerRecoveryData.SystemAliveReceived(timestamp.Received, timestamp.Created);
    }

    private void ProducerUp(ProducerRecoveryData producerRecoveryData, ProducerUpReason reason)
    {
        if (producerRecoveryData.IsDisabled)
        {
            return;
        }

        if (producerRecoveryData.IsFlaggedDown)
        {
            producerRecoveryData.SetProducerUp();
        }

        NotifyProducerChangedState(producerRecoveryData, UpReasonToProducerReason(reason));
    }

    private void MakeSnapshotRecovery(ProducerRecoveryData producerRecoveryData, long? fromTimestamp)
    {
        if (!_isOpened)
        {
            return;
        }

        var now = Timestamp.Now();
        long recoveryFrom = 0;
        if (fromTimestamp != null)
        {
            recoveryFrom = (long)fromTimestamp;
        }

        if (recoveryFrom == 0 && _oddsFeedConfiguration.InitialSnapshotTimeInMinutes != default)
        {
            recoveryFrom = now - Timestamp.FromMinutes(_oddsFeedConfiguration.InitialSnapshotTimeInMinutes);
        }

        var maxRecoveryFrom = now - Timestamp.FromMinutes(producerRecoveryData.StatefulRecoveryWindowInMinutes);
        if (recoveryFrom == 0 || recoveryFrom < maxRecoveryFrom)
        {
            recoveryFrom = maxRecoveryFrom;
        }

        var requestId = _requestIdFactory.GetNext();
        var producerName = producerRecoveryData.ProducerName ??
                           throw new SdkException($"Cannot find producer for {producerRecoveryData.ProducerId}");

        producerRecoveryData.SetProducerRecoveryState(requestId, now, RecoveryState.Started);

        _log.LogInformation($"Recovery started for request {requestId}");


        bool success;
        try
        {
            var task = _apiClient.PostRecoveryRequest(producerName, requestId, _oddsFeedConfiguration.NodeId,
                recoveryFrom);
            task.Wait();
            success = task.Result;
        }
        catch (Exception e)
        {
            _log.LogError($"Recovery failed with {e}");
            e.HandleAccordingToStrategy(GetType().Name, _log, _oddsFeedConfiguration.ExceptionHandlingStrategy);
            success = false;
        }

        var producer = (Producer)_producerManager.Get(producerRecoveryData.ProducerId);
        producer.ProducerData.LastRecoveryInfo =
            new RecoveryInfo(recoveryFrom, now, requestId, _oddsFeedConfiguration.NodeId, success);
    }

    private ProducerRecoveryData FindOrMakeProducerRecoveryData(int producerId) =>
        _producerRecoveryData.GetOrAdd(producerId, l => new ProducerRecoveryData(l, _producerManager));

    private void TimerTick(object sender)
    {
        lock (_lock)
        {
            var now = Timestamp.Now();

            foreach (var it in _producerRecoveryData)
            {
                var producerRecoveryData = it.Value;
                if (producerRecoveryData.IsDisabled)
                {
                    continue;
                }

                var aliveInterval = now - ( producerRecoveryData.LastSystemAliveReceivedTimestamp ?? 0L );

                if (aliveInterval > Timestamp.FromSeconds(_oddsFeedConfiguration.MaxInactivitySeconds))
                {
                    ProducerDown(
                        producerRecoveryData,
                        ProducerDownReason.AliveIntervalViolation
                    );
                }
                else if (!CalculateTiming(producerRecoveryData, now))
                {
                    ProducerDown(
                        producerRecoveryData,
                        ProducerDownReason.ProcessingQueueDelayViolation
                    );
                }
            }
        }
    }

    private void ProducerDown(ProducerRecoveryData producerRecoveryData, ProducerDownReason downReason)
    {
        if (producerRecoveryData.IsDisabled)
        {
            return;
        }

        if (producerRecoveryData.IsFlaggedDown && producerRecoveryData.ProducerDownReason != downReason)
        {
            _log.LogInformation(
                $"Changing producer {producerRecoveryData.ProducerName} down reason from {producerRecoveryData.ProducerDownReason} to {downReason}");
            producerRecoveryData.SetProducerDown(downReason);
        }

        if (producerRecoveryData.RecoveryState == RecoveryState.Started &&
            downReason != ProducerDownReason.ProcessingQueueDelayViolation)
        {
            producerRecoveryData.InterruptProducerRecovery();
        }

        if (!producerRecoveryData.IsFlaggedDown)
        {
            producerRecoveryData.SetProducerDown(downReason);
        }

        NotifyProducerChangedState(producerRecoveryData, DownReasonToProducerReason(downReason));
    }

    private void NotifyProducerChangedState(ProducerRecoveryData producerRecoveryData, ProducerStatusReason reason)
    {
        if (producerRecoveryData.ProducerStatusReason == reason)
        {
            return;
        }

        producerRecoveryData.ProducerStatusReason = reason;

        var now = Timestamp.Now();

        var messageTimestamp = new MessageTimestamp(now);
        var eventArgs = new ProducerStatusChangeEventArgs(new ProducerStatusChange(
            _producerManager.Get(producerRecoveryData.ProducerId),
            messageTimestamp,
            producerRecoveryData.IsFlaggedDown,
            !CalculateTiming(producerRecoveryData, now),
            reason
        ));

        if (producerRecoveryData.IsFlaggedDown)
        {
            Dispatch(EventProducerDown, eventArgs, nameof(EventProducerDown));
        }
        else
        {
            Dispatch(EventProducerUp, eventArgs, nameof(EventProducerUp));
        }
    }

    private bool CalculateTiming(ProducerRecoveryData producerRecoveryData, long timestamp)
    {
        var maxInactivity = Timestamp.FromSeconds(_oddsFeedConfiguration.MaxInactivitySeconds);
        var messageProcessingDelay = timestamp - producerRecoveryData.LastProcessedMessageGenTimestamp;
        var userAliveDelay = timestamp - ( producerRecoveryData.LastUserSessionAliveReceivedTimestamp ?? 0L );

        return Math.Abs(messageProcessingDelay) < maxInactivity && Math.Abs(userAliveDelay) < maxInactivity;
    }

    private static ProducerStatusReason DownReasonToProducerReason(ProducerDownReason reason) =>
        reason switch
        {
            ProducerDownReason.AliveIntervalViolation => ProducerStatusReason.DownAliveIntervalViolation,
            ProducerDownReason.ProcessingQueueDelayViolation => ProducerStatusReason
                .DownProcessingQueueDelayViolation,
            ProducerDownReason.Other => ProducerStatusReason.DownOther,
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unknown ProducerDownReason")
        };

    private static ProducerStatusReason UpReasonToProducerReason(ProducerUpReason reason) =>
        reason switch
        {
            ProducerUpReason.FirstRecoveryCompleted => ProducerStatusReason.UpFirstRecoveryCompleted,
            ProducerUpReason.ProcessingQueueDelayStabilized =>
                ProducerStatusReason.UpProcessingQueueDelayStabilized,
            ProducerUpReason.ReturnedFromInactivity => ProducerStatusReason.UpReturnedFromInactivity,
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unknown ProducerUpReason")
        };
}

internal class ProducerRecoveryData
{
    private readonly ConcurrentDictionary<long, EventRecovery> _eventRecoveries;
    private readonly IProducerManager _producerManager;
    internal readonly int ProducerId;

    private RecoveryData _currentRecovery;
    internal bool FirstRecoveryCompleted;
    internal long? LastSystemAliveReceivedTimestamp;
    internal long? LastUserSessionAliveReceivedTimestamp;
    internal long? LastValidAliveGenTimestampInRecovery;
    internal ProducerDownReason? ProducerDownReason;
    internal ProducerStatusReason? ProducerStatusReason;
    internal RecoveryState? RecoveryState;

    internal ProducerRecoveryData(int producerId, IProducerManager producerManager)
    {
        ProducerId = producerId;
        _producerManager = producerManager;
        _eventRecoveries = new ConcurrentDictionary<long, EventRecovery>();
    }

    internal long? LastRecoveryStartedAt => _currentRecovery?.RecoveryStartedAt;
    internal bool IsFlaggedDown => _producerManager.Get(ProducerId).IsProducerDown;

    internal bool IsDisabled
    {
        get
        {
            var producer = _producerManager.Get(ProducerId);
            return producer.IsDisabled || !producer.IsAvailable;
        }
    }

    internal bool IsPerformingRecovery =>
        RecoveryState is Recovery.RecoveryState.Started or Recovery.RecoveryState.Interrupted;

    internal long TimestampForRecovery => _producerManager.Get(ProducerId).TimestampForRecovery;
    internal string ProducerName => _producerManager.Get(ProducerId).Name;

    internal int StatefulRecoveryWindowInMinutes =>
        _producerManager.Get(ProducerId).StatefulRecoveryWindowInMinutes;


    internal long LastMessageReceivedTimestamp
    {
        get
        {
            var producer = (Producer)_producerManager.Get(ProducerId);
            return producer.LastMessageTimestamp;
        }
        set
        {
            var producer = ( (Producer)_producerManager.Get(ProducerId) ).ProducerData;
            producer.LastMessageTimestamp = value;
        }
    }

    internal long LastProcessedMessageGenTimestamp
    {
        get
        {
            var producer = (Producer)_producerManager.Get(ProducerId);
            return producer.LastProcessedMessageGenTimestamp;
        }
        set
        {
            var producer = ( (Producer)_producerManager.Get(ProducerId) ).ProducerData;
            producer.LastProcessedMessageGenTimestamp = value;
        }
    }

    internal void EventRecoveryCompleted(long recoveryId) => _eventRecoveries.TryRemove(recoveryId, out _);

    internal void SystemAliveReceived(long receivedTimestamp, long aliveGenTimestamp)
    {
        LastSystemAliveReceivedTimestamp = receivedTimestamp;
        if (!IsFlaggedDown)
        {
            var producer = ( (Producer)_producerManager.Get(ProducerId) ).ProducerData;
            producer.LastAliveReceivedGenTimestamp = aliveGenTimestamp;
        }

        if (RecoveryState == Recovery.RecoveryState.Started)
        {
            LastValidAliveGenTimestampInRecovery = aliveGenTimestamp;
        }
    }

    internal bool ValidateSnapshotComplete(long recoveryId, MessageInterest messageInterest)
    {
        if (!IsPerformingRecovery)
        {
            return false;
        }

        if (_currentRecovery == null || _currentRecovery.RecoveryId != recoveryId)
        {
            return false;
        }

        if (!IsSnapshotValidationNeeded(messageInterest))
        {
            return true;
        }

        var interests = _currentRecovery.SnapshotComplete(messageInterest);
        return ValidateProducerSnapshotCompletes(interests);
    }

    internal bool ValidateEventSnapshotComplete(long recoveryId, MessageInterest messageInterest)
    {
        var found = _eventRecoveries.TryGetValue(recoveryId, out var eventRecovery);
        if (!found || eventRecovery == null)
        {
            return false;
        }

        if (IsSnapshotValidationNeeded(messageInterest))
        {
            return false;
        }

        var interests = eventRecovery.SnapshotComplete(messageInterest);
        return ValidateProducerSnapshotCompletes(interests);
    }


    internal bool IsKnownRecovery(long requestId) =>
        _currentRecovery?.RecoveryId == requestId || _eventRecoveries.ContainsKey(requestId);

    internal EventRecovery GetEventRecovery(long recoveryId)
    {
        var found = _eventRecoveries.TryGetValue(recoveryId, out var eventRecovery);
        return found ? eventRecovery : null;
    }

    internal void SetProducerRecoveryState(
        long recoveryId,
        long recoveryStartedAt,
        RecoveryState recoveryState
    )
    {
        RecoveryState = recoveryState;
        _currentRecovery = new RecoveryData(recoveryId, recoveryStartedAt);
    }

    internal void InterruptProducerRecovery() => RecoveryState = Recovery.RecoveryState.Interrupted;

    internal void SetProducerDown(ProducerDownReason producerDownReason)
    {
        var producer = ( (Producer)_producerManager.Get(ProducerId) ).ProducerData;
        producer.FlaggedDown = true;
        ProducerDownReason = producerDownReason;
        _eventRecoveries.Clear();
    }

    internal void SetProducerUp()
    {
        var producer = ( (Producer)_producerManager.Get(ProducerId) ).ProducerData;
        producer.FlaggedDown = false;
        ProducerDownReason = null;
    }


    internal void SetEventRecoveryState(URN eventId, long recoveryId, long recoveryStartedAt)
    {
        if (recoveryId == 0 && recoveryStartedAt == 0)
        {
            _eventRecoveries.TryRemove(recoveryId, out _);
        }
        else
        {
            var eventRecovery = new EventRecovery(eventId, recoveryId, recoveryStartedAt);
            _eventRecoveries.AddOrUpdate(recoveryId, eventRecovery, (_, _) => eventRecovery);
        }
    }

    private static bool IsSnapshotValidationNeeded(MessageInterest messageInterest) =>
        messageInterest.MessageInterestType == MessageInterest.LiveMessagesOnly.MessageInterestType ||
        messageInterest.MessageInterestType == MessageInterest.PrematchMessagesOnly.MessageInterestType;

    private bool ValidateProducerSnapshotCompletes(
        ConcurrentDictionary<MessageInterest, bool> receivedSnapshotCompletes)
    {
        var producer = (Producer)_producerManager.Get(ProducerId);

        var notFinished = producer.ProducerScopes?.Select(scope =>
        {
            return scope switch
            {
                ProducerScope.Live =>
                    receivedSnapshotCompletes.ContainsKey(MessageInterest.LiveMessagesOnly),
                ProducerScope.Prematch => receivedSnapshotCompletes.TryGetValue(
                    MessageInterest.PrematchMessagesOnly, out _),
                _ => true
            };
        }).Contains(false);
        return (bool)( notFinished != null ? !notFinished : true );
    }
}

internal class RecoveryData
{
    private readonly ConcurrentDictionary<MessageInterest, bool> _interestsOfSnapshotComplete;
    internal readonly long RecoveryId;
    internal readonly long RecoveryStartedAt;

    internal RecoveryData(long recoveryId, long recoveryStartedAt)
    {
        RecoveryId = recoveryId;
        RecoveryStartedAt = recoveryStartedAt;
        _interestsOfSnapshotComplete = new ConcurrentDictionary<MessageInterest, bool>();
    }

    internal ConcurrentDictionary<MessageInterest, bool> SnapshotComplete(MessageInterest messageInterest)
    {
        _interestsOfSnapshotComplete.TryAdd(messageInterest, true);
        return _interestsOfSnapshotComplete;
    }
}

internal class EventRecovery : RecoveryData
{
    internal readonly URN EventId;

    internal EventRecovery(URN eventId, long recoveryId, long recoveryStartedAt) :
        base(recoveryId, recoveryStartedAt) => EventId = eventId;
}

internal enum RecoveryState
{
    NotStarted = 0,
    Started = 1,
    Completed = 2,
    Interrupted = 3,
    Error = 4
}

public enum ProducerDownReason
{
    AliveIntervalViolation = 0,
    ProcessingQueueDelayViolation = 1,
    Other = 2
}

public enum ProducerUpReason
{
    FirstRecoveryCompleted = 0,
    ProcessingQueueDelayStabilized = 1,
    ReturnedFromInactivity = 2
}

public enum ProducerStatusReason
{
    DownAliveIntervalViolation = 0,
    DownProcessingQueueDelayViolation = 1,
    DownOther = 2,
    UpFirstRecoveryCompleted = 3,
    UpProcessingQueueDelayStabilized = 4,
    UpReturnedFromInactivity = 5
}