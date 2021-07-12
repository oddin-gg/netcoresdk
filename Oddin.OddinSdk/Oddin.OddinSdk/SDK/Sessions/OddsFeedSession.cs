using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common.Exceptions;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using Oddin.OddinSdk.SDK.Sessions.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Sessions
{
    internal class OddsFeedSession : DispatcherBase, IOddsFeedSession, IOpenable
    {
        private readonly IAmqpClient _amqpClient;
        private readonly IFeedMessageMapper _feedMessageMapper;
        private readonly MessageInterest _messageInterest;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private bool _isOpened;
        private readonly object _isOpenedLock = new object();

        public string Name { get; }

        public OddsFeedSession(
            ILoggerFactory loggerFactory,
            IAmqpClient amqpClient,
            IFeedMessageMapper feedMessageMapper,
            MessageInterest messageInterest,
            ExceptionHandlingStrategy exceptionHandlingStrategy)
            : base(loggerFactory)
        {
            if (amqpClient is null)
                throw new ArgumentNullException(nameof(amqpClient));

            if (feedMessageMapper is null)
                throw new ArgumentNullException(nameof(feedMessageMapper));

            if (messageInterest is null)
                throw new ArgumentNullException(nameof(messageInterest));

            _amqpClient = amqpClient;
            _feedMessageMapper = feedMessageMapper;
            _messageInterest = messageInterest;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;

            Name = messageInterest.Name;
        }

        public event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
        public event EventHandler<OddsChangeEventArgs<ISportEvent>> OnOddsChange;
        public event EventHandler<BetStopEventArgs<ISportEvent>> OnBetStop;

        private void HandleUnparsableMessageReceived(object sender, UnparsableMessageEventArgs eventArgs)
        {
            Dispatch(OnUnparsableMessageReceived, eventArgs, nameof(OnUnparsableMessageReceived));
        }

        private void HandleAliveMessageReceived(object sender, SimpleMessageEventArgs<alive> eventArgs)
        {
            // TODO: implement heartbeat handling (and potential recovery?)
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
                if (_exceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    throw;
                else
                    return;
            }
        }

        private void CreateAndDispatchOddsChange(object sender, SimpleMessageEventArgs<odds_change> eventArgs)
        {
            var oddsChangeEventArgs = new OddsChangeEventArgs<ISportEvent>(_feedMessageMapper, eventArgs.FeedMessage, eventArgs.RawMessage);
            Dispatch(OnOddsChange, oddsChangeEventArgs, nameof(OnOddsChange));
        }

        private void CreateAndDispatchBetStop(object sender, SimpleMessageEventArgs<bet_stop> eventArgs)
        {
            var betStopEventArgs = new BetStopEventArgs<ISportEvent>(_feedMessageMapper, eventArgs.FeedMessage, eventArgs.RawMessage);
            Dispatch(OnBetStop, betStopEventArgs, nameof(OnBetStop));
        }

        private void HandleOddsChangeMessageReceived(object sender, SimpleMessageEventArgs<odds_change> eventArgs)
            => CreateAndDispatchFeedMessageEventArgs<OddsChangeEventArgs<ISportEvent>, odds_change>(CreateAndDispatchOddsChange, sender, eventArgs);

        private void HandleBetStopMessageReceived(object sender, SimpleMessageEventArgs<bet_stop> eventArgs)
            => CreateAndDispatchFeedMessageEventArgs<BetStopEventArgs<ISportEvent>, bet_stop>(CreateAndDispatchBetStop, sender, eventArgs);

        private void AttachAmqpClientEvents()
        {
            _amqpClient.UnparsableMessageReceived += HandleUnparsableMessageReceived;
            _amqpClient.AliveMessageReceived += HandleAliveMessageReceived;
            _amqpClient.OddsChangeMessageReceived += HandleOddsChangeMessageReceived;
            _amqpClient.BetStopMessageReceived += HandleBetStopMessageReceived;
        }

        private void DetachAmqpClintEvents()
        {
            _amqpClient.UnparsableMessageReceived -= HandleUnparsableMessageReceived;
            _amqpClient.AliveMessageReceived -= HandleAliveMessageReceived;
            _amqpClient.OddsChangeMessageReceived -= HandleOddsChangeMessageReceived;
            _amqpClient.BetStopMessageReceived -= HandleBetStopMessageReceived;
        }

        /// <summary>
        /// Checks if this instance of <see cref="Feed"/> is NOT marked as opened and marks it as such in a thread-safe way
        /// </summary>
        /// <returns><see langword="false"/> if this instance of <see cref="Feed"/> was already marked as opened</returns>
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
                _amqpClient.Connect(_messageInterest);
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
