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
using Oddin.OddsFeedSdk.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.Sessions
{
    internal class OddsFeedSession : DispatcherBase, IOddsFeedSession, IOpenable
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(OddsFeedSession));

        private readonly IAmqpClient _amqpClient;
        private readonly IFeedMessageMapper _feedMessageMapper;
        private readonly IFeedConfiguration _configuration;
        private bool _isOpened;
        private readonly object _isOpenedLock = new object();

        internal MessageInterest MessageInterest { get; }

        public string Name { get; }

        public IOddsFeed Feed { get; }

        public OddsFeedSession(
            IOddsFeed feed,
            IAmqpClient amqpClient,
            IFeedMessageMapper feedMessageMapper,
            MessageInterest messageInterest,
            IFeedConfiguration configuration)
        {
            _amqpClient = amqpClient ?? throw new ArgumentNullException(nameof(amqpClient));
            _feedMessageMapper = feedMessageMapper ?? throw new ArgumentNullException(nameof(feedMessageMapper));
            MessageInterest = messageInterest ?? throw new ArgumentNullException(nameof(messageInterest));
            _configuration = configuration;

            Name = messageInterest.Name;
            Feed = feed;
        }

        public event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
        public event EventHandler<OddsChangeEventArgs<ISportEvent>> OnOddsChange;
        public event EventHandler<BetStopEventArgs<ISportEvent>> OnBetStop;
        public event EventHandler<BetSettlementEventArgs<ISportEvent>> OnBetSettlement;
        public event EventHandler<BetCancelEventArgs<ISportEvent>> OnBetCancel;
        public event EventHandler<FixtureChangeEventArgs<ISportEvent>> OnFixtureChange;

        private void HandleUnparsableMessageReceived(object sender, UnparsableMessageEventArgs eventArgs)
        {
            Dispatch(OnUnparsableMessageReceived, eventArgs, nameof(OnUnparsableMessageReceived));
        }

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
                else
                    return;
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

        private void HandleOddsChangeMessageReceived(object sender, SimpleMessageEventArgs<odds_change> eventArgs)
            => CreateAndDispatchFeedMessageEventArgs<OddsChangeEventArgs<ISportEvent>, odds_change>(CreateAndDispatchOddsChange, sender, eventArgs);

        private void HandleBetStopMessageReceived(object sender, SimpleMessageEventArgs<bet_stop> eventArgs)
            => CreateAndDispatchFeedMessageEventArgs<BetStopEventArgs<ISportEvent>, bet_stop>(CreateAndDispatchBetStop, sender, eventArgs);

        private void HandleBetSettlementMessageReceived(object sender, SimpleMessageEventArgs<bet_settlement> eventArgs)
            => CreateAndDispatchFeedMessageEventArgs<BetSettlementEventArgs<ISportEvent>, bet_settlement>(CreateAndDispatchBetSettlement, sender, eventArgs);

        private void HandleBetCancelMessageReceived(object sender, SimpleMessageEventArgs<bet_cancel> eventArgs)
            => CreateAndDispatchFeedMessageEventArgs<BetCancelEventArgs<ISportEvent>, bet_cancel>(CreateAndDispatchBetCancel, sender, eventArgs);

        private void HandleFixtureChangeMessageReceived(object sender, SimpleMessageEventArgs<fixture_change> eventArgs)
            => CreateAndDispatchFeedMessageEventArgs<FixtureChangeEventArgs<ISportEvent>, fixture_change>(CreateAndDispatchFixtureChange, sender, eventArgs);

        private void AttachAmqpClientEvents()
        {
            _amqpClient.UnparsableMessageReceived += HandleUnparsableMessageReceived;
            _amqpClient.OddsChangeMessageReceived += HandleOddsChangeMessageReceived;
            _amqpClient.BetStopMessageReceived += HandleBetStopMessageReceived;
            _amqpClient.BetSettlementMessageReceived += HandleBetSettlementMessageReceived;
            _amqpClient.BetCancelMessageReceived += HandleBetCancelMessageReceived;
            _amqpClient.FixtureChangeMessageReceived += HandleFixtureChangeMessageReceived;

        }

        private void DetachAmqpClintEvents()
        {
            _amqpClient.UnparsableMessageReceived -= HandleUnparsableMessageReceived;
            _amqpClient.OddsChangeMessageReceived -= HandleOddsChangeMessageReceived;
            _amqpClient.BetStopMessageReceived -= HandleBetStopMessageReceived;
            _amqpClient.BetSettlementMessageReceived -= HandleBetSettlementMessageReceived;
            _amqpClient.BetCancelMessageReceived -= HandleBetCancelMessageReceived;
            _amqpClient.FixtureChangeMessageReceived -= HandleFixtureChangeMessageReceived;
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

        public void Open()
        {
            if (TrySetAsOpened() == false)
                throw new InvalidOperationException($"Cannot open an instance of {typeof(OddsFeedSession).Name} that is already opened!");

            AttachAmqpClientEvents();
            try
            {
                _amqpClient.Connect(MessageInterest);
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
            DetachAmqpClintEvents();
            SetAsClosed();
        }
    }
}