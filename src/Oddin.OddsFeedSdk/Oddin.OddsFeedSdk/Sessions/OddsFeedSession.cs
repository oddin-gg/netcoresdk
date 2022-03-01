using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Oddin.OddsFeedSdk.Abstractions;
using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Managers.Recovery;
using RabbitMQ.Client.Events;

namespace Oddin.OddsFeedSdk.Sessions
{
    internal class OddsFeedSession : DispatcherBase, IOddsFeedSession
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(OddsFeedSession));

        internal readonly Guid SessionId = Guid.NewGuid();

        private readonly IAmqpClient _amqpClient;
        private readonly IFeedMessageMapper _feedMessageMapper;
        private readonly IFeedConfiguration _configuration;
        private bool _isOpened;
        private readonly object _isOpenedLock = new object();
        private readonly IProducerManager _producerManager;
        private readonly RecoveryManager _recoveryMessageProcessor;
        private readonly CacheManager _cacheManager;

        internal MessageInterest MessageInterest { get; }

        public string Name { get; }

        public IOddsFeed Feed { get; }

        public OddsFeedSession(IOddsFeed feed,
            IAmqpClient amqpClient,
            IFeedMessageMapper feedMessageMapper,
            MessageInterest messageInterest,
            IFeedConfiguration configuration,
            IProducerManager producerManager,
            RecoveryManager recoveryMessageProcessor,
            CacheManager cacheManager
            )
        {
            _amqpClient = amqpClient ?? throw new ArgumentNullException(nameof(amqpClient));
            _feedMessageMapper = feedMessageMapper ?? throw new ArgumentNullException(nameof(feedMessageMapper));
            MessageInterest = messageInterest ?? throw new ArgumentNullException(nameof(messageInterest));
            _configuration = configuration;
            _producerManager = producerManager;
            _recoveryMessageProcessor = recoveryMessageProcessor;
            _cacheManager = cacheManager;

            Name = messageInterest.Name;
            Feed = feed;
        }

        public event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
        public event EventHandler<OddsChangeEventArgs<ISportEvent>> OnOddsChange;
        public event EventHandler<BetStopEventArgs<ISportEvent>> OnBetStop;
        public event EventHandler<BetSettlementEventArgs<ISportEvent>> OnBetSettlement;
        public event EventHandler<BetCancelEventArgs<ISportEvent>> OnBetCancel;
        public event EventHandler<FixtureChangeEventArgs<ISportEvent>> OnFixtureChange;

        private void CreateAndDispatchFeedMessageEventArgs<TMessageEventArgs, TMessage>(Action<object, SimpleMessageEventArgs<TMessage>> createAndDispatch, object sender, SimpleMessageEventArgs<TMessage> eventArgs)
        {
            try
            {
                createAndDispatch(sender, eventArgs);
            }
            catch (Exception e)
            {
                var message = $"An exception was thrown when creating an object of type {typeof(TMessageEventArgs).Name} from a message received form AMQP feed!";
                _log.LogError($"{message} Exception: {e}");
                if (_configuration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    throw;
            }
        }

        private void CreateAndDispatchOddsChange(object sender, SimpleMessageEventArgs<odds_change> eventArgs)
        {
            var oddsChangeEventArgs = new OddsChangeEventArgs<ISportEvent>(
                _feedMessageMapper,
                eventArgs.FeedMessage,
                new[] { _configuration.DefaultLocale },
                eventArgs.RawMessage);

            Dispatch(OnOddsChange, oddsChangeEventArgs, nameof(OnOddsChange));
        }

        private void CreateAndDispatchBetStop(object sender, SimpleMessageEventArgs<bet_stop> eventArgs)
        {
            var betStopEventArgs = new BetStopEventArgs<ISportEvent>(
                _feedMessageMapper,
                eventArgs.FeedMessage,
                new[] { _configuration.DefaultLocale },
                eventArgs.RawMessage);

            Dispatch(OnBetStop, betStopEventArgs, nameof(OnBetStop));
        }

        private void CreateAndDispatchBetSettlement(object sender, SimpleMessageEventArgs<bet_settlement> eventArgs)
        {
            var betSettlementEventArgs = new BetSettlementEventArgs<ISportEvent>(
                _feedMessageMapper,
                eventArgs.FeedMessage,
                new[] { _configuration.DefaultLocale },
                eventArgs.RawMessage);

            Dispatch(OnBetSettlement, betSettlementEventArgs, nameof(OnBetSettlement));
        }

        private void CreateAndDispatchBetCancel(object sender, SimpleMessageEventArgs<bet_cancel> eventArgs)
        {
            var betCancelEventArgs = new BetCancelEventArgs<ISportEvent>(
                _feedMessageMapper,
                eventArgs.FeedMessage,
                new[] { _configuration.DefaultLocale },
                eventArgs.RawMessage);

            Dispatch(OnBetCancel, betCancelEventArgs, nameof(OnBetCancel));
        }

        private void CreateAndDispatchFixtureChange(object sender, SimpleMessageEventArgs<fixture_change> eventArgs)
        {
            var fixtureChangeEventArgs = new FixtureChangeEventArgs<ISportEvent>(
                _feedMessageMapper,
                eventArgs.FeedMessage,
                new[] { _configuration.DefaultLocale },
                eventArgs.RawMessage);

            Dispatch(OnFixtureChange, fixtureChangeEventArgs, nameof(OnFixtureChange));
        }

        private void PublishUnparsableMessage(byte[] messageBody, string messageRoutingKey)
        {
            MessageType messageType = MessageType.UNKNOWN;
            string producer = string.Empty;
            string eventId = string.Empty;
            try
            {
                messageType = TopicParsingHelper.GetMessageType(messageRoutingKey);
                producer = TopicParsingHelper.GetProducer(messageRoutingKey);
                eventId = TopicParsingHelper.GetEventId(messageRoutingKey);
            }
            catch (ArgumentException)
            {
                if (_configuration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    throw;
            }
            Dispatch(OnUnparsableMessageReceived, new UnparsableMessageEventArgs(messageType, producer, eventId, messageBody), nameof(OnUnparsableMessageReceived));
        }

        private bool TryGetMessageSentTime(BasicDeliverEventArgs eventArgs, out long sentTime)
        {
            sentTime = 0;

            var headers = eventArgs?.BasicProperties?.Headers;
            if (headers is null)
                return false;

            var sentAtHeader = "timestamp_in_ms";
            if (headers.ContainsKey(sentAtHeader) == false)
                return false;

            var value = headers[sentAtHeader].ToString();
            if (long.TryParse(value, out var parseResult) == false)
                return false;

            sentTime = parseResult;
            return true;
        }

        private void SetMessageTimes(FeedMessageModel message, BasicDeliverEventArgs eventArgs, long receivedAt)
        {
            message.ReceivedAt = receivedAt;
            message.SentAt = TryGetMessageSentTime(eventArgs, out var sentTime) == false ? message.GeneratedAt : sentTime;
        }

        private bool FilterFeedMessage(FeedMessageModel feedMessage, MessageInterest messageInterest)
        {
            var producerId = feedMessage.ProducerId;
            var producer = (Producer) _producerManager.Get(producerId);
            if (producer == null)
            {
                _log.LogDebug($"Unknown producer {producerId} sending message - {feedMessage}");
                return false;
            }
            if (producer.IsDisabled || !producer.IsAvailable)
            {
                return false;
            }
            return messageInterest.IsProducerInScope(producer);
        }

        private bool FilterFixtureChanges(FeedMessageModel message)
        {
            switch (message)
            {
                case fixture_change fixtureChange:
                    var messageKey = fixtureChange.Key();
                    return _cacheManager.DispatchedFixtureChanges.Contains(messageKey) == false;
            }
            return true;
        }

        private void OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var receivedAt = Timestamp.Now();
            var body = eventArgs.Body.ToArray();
            var xml = Encoding.UTF8.GetString(body);
            var success = FeedMessageDeserializer.TryDeserializeMessage(xml, out var message);

            if (success == false)
            {
                PublishUnparsableMessage(body, eventArgs.RoutingKey);
                return;
            }

            SetMessageTimes(message, eventArgs, receivedAt);

            if (!FilterFeedMessage(message, MessageInterest) || !FilterFixtureChanges(message))
            {
                return;
            }

            var producerId = message.ProducerId;
            _recoveryMessageProcessor.OnMessageProcessingStarted(
                SessionId,
                producerId,
                receivedAt
            );

            _cacheManager.OnFeedMessageReceived(message);

            long? timestamp = null;

            switch (message)
            {
                case fixture_change fixtureChange:
                    var messageKey = fixtureChange.Key();
                    _cacheManager.DispatchedFixtureChanges.Add(messageKey);
                    break;
                case alive aliveMessage:
                    timestamp = aliveMessage.GeneratedAt;
                    _recoveryMessageProcessor.OnAliveReceived(sender, new AliveEventArgs(aliveMessage, MessageInterest));
                    break;
                case snapshot_complete snapshotComplete:
                    _recoveryMessageProcessor.OnSnapshotCompleteReceived(sender, new SnapshotCompleteEventArgs(snapshotComplete, MessageInterest));
                    break;
            }

            switch (message)
            {
                case alive:
                    break;
                case snapshot_complete:
                    break;
                case bet_stop betStopMessage:
                    timestamp = betStopMessage.GeneratedAt;
                    var betStopMessageArgs = new SimpleMessageEventArgs<bet_stop>(betStopMessage, body);
                    CreateAndDispatchFeedMessageEventArgs<BetStopEventArgs<ISportEvent>, bet_stop>(CreateAndDispatchBetStop, sender, betStopMessageArgs);
                    break;
                case bet_cancel betCancel:
                    var betCancelMessageArgs = new SimpleMessageEventArgs<bet_cancel>(betCancel, body);
                    CreateAndDispatchFeedMessageEventArgs<BetCancelEventArgs<ISportEvent>, bet_cancel>(CreateAndDispatchBetCancel, sender, betCancelMessageArgs);
                    break;
                case bet_settlement betSettlement:
                    var betSettlementMessageArgs = new SimpleMessageEventArgs<bet_settlement>(betSettlement, body);
                    CreateAndDispatchFeedMessageEventArgs<BetSettlementEventArgs<ISportEvent>, bet_settlement>(CreateAndDispatchBetSettlement, sender, betSettlementMessageArgs);
                    break;
                case fixture_change fixtureChange:
                    var fixtureChangeMessageArgs = new SimpleMessageEventArgs<fixture_change>(fixtureChange, body);
                    CreateAndDispatchFeedMessageEventArgs<FixtureChangeEventArgs<ISportEvent>, fixture_change>(CreateAndDispatchFixtureChange, sender, fixtureChangeMessageArgs);
                    break;

                case odds_change oddsChangeMessage:
                    timestamp = oddsChangeMessage.GeneratedAt;
                    var oddsChangeMessageArgs = new SimpleMessageEventArgs<odds_change>(oddsChangeMessage, body);
                    CreateAndDispatchFeedMessageEventArgs<OddsChangeEventArgs<ISportEvent>, odds_change>(CreateAndDispatchOddsChange, sender, oddsChangeMessageArgs);
                    break;
                default:
                    PublishUnparsableMessage(body, eventArgs.RoutingKey);
                    break;
            }

            _recoveryMessageProcessor.OnMessageProcessingEnded(
                SessionId,
                producerId,
                timestamp
            );

        }

        private void AttachEvents()
        {
            _amqpClient.OnReceived += OnReceived;
        }

        private void DetachEvents()
        {
            _amqpClient.OnReceived -= OnReceived;
        }

        private bool TrySetAsOpened()
        {
            lock (_isOpenedLock)
            {
                if (_isOpened)
                    return false;

                _isOpened = true;
                return true;
            }
        }

        private void SetAsClosed()
        {
            lock (_isOpenedLock)
            {
                _isOpened = false;
            }
        }

        public bool IsOpened()
        {
            lock (_isOpenedLock)
            {
                return _isOpened;
            }
        }

        public void Open(IEnumerable<string> routingKeys)
        {
            if (TrySetAsOpened() == false)
                throw new InvalidOperationException($"Cannot open an instance of {nameof(OddsFeedSession)} that is already opened!");

            AttachEvents();
            try
            {
                _amqpClient.Connect(MessageInterest, routingKeys);
            }
            catch (CommunicationException)
            {
                SetAsClosed();
                throw;
            }
        }

        public void Close()
        {
            _amqpClient.Disconnect();
            DetachEvents();
            SetAsClosed();
        }
    }
}