using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.Sessions.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddinSdk.SDK.Sessions
{
    internal class OddsFeedSession : DispatcherBase, IOddsFeedSession, IOpenable
    {
        private readonly IAmqpClient _amqpClient;
        private readonly IFeedMessageMapper _feedMessageMapper;
        private readonly MessageInterest _messageInterest;
        private readonly IEnumerable<CultureInfo> _defaultCultures;
        private bool _isOpened;
        private readonly object _isOpenedLock = new object();

        public string Name { get; }

        public OddsFeedSession(
            ILoggerFactory loggerFactory,
            IAmqpClient amqpClient,
            IFeedMessageMapper feedMessageMapper,
            MessageInterest messageInterest,
            IEnumerable<CultureInfo> defaultCultures)
            : base(loggerFactory)
        {
            _amqpClient = amqpClient;
            _feedMessageMapper = feedMessageMapper;
            _messageInterest = messageInterest;
            _defaultCultures = defaultCultures as IReadOnlyCollection<CultureInfo>;

            Name = messageInterest.Name;
        }

        public event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
        public event EventHandler<OddsChangeEventArgs<ISportEvent>> OnOddsChange;

        private void HandleUnparsableMessageReceived(object sender, UnparsableMessageEventArgs eventArgs)
        {
            Dispatch(OnUnparsableMessageReceived, eventArgs, nameof(OnUnparsableMessageReceived));
        }

        private void HandleAliveMessageReceived(object sender, SimpleMessageEventArgs<alive> eventArgs)
        {
            // TODO: implement heartbeat handling (and potential recovery?)
        }

        private void HandleOddsChangeMessageReceived(object sender, SimpleMessageEventArgs<odds_change> eventArgs)
        {
            var oddsChangeEventArgs = new OddsChangeEventArgs<ISportEvent>(_feedMessageMapper, eventArgs.FeedMessage, _defaultCultures, eventArgs.RawMessage);
            Dispatch(OnOddsChange, oddsChangeEventArgs, nameof(OnOddsChange));
        }

        private void AttachAmqpClientEvents()
        {
            _amqpClient.UnparsableMessageReceived += HandleUnparsableMessageReceived;
            _amqpClient.AliveMessageReceived += HandleAliveMessageReceived;
            _amqpClient.OddsChangeMessageReceived += HandleOddsChangeMessageReceived;
        }

        private void DetachAmqpClintEvents()
        {
            _amqpClient.UnparsableMessageReceived -= HandleUnparsableMessageReceived;
            _amqpClient.AliveMessageReceived -= HandleAliveMessageReceived;
            _amqpClient.OddsChangeMessageReceived -= HandleOddsChangeMessageReceived;
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

        public void Open()
        {
            if (TrySetAsOpened() == false)
                throw new InvalidOperationException($"Cannot open an instance of {typeof(OddsFeedSession).Name} that is already opened!");

            AttachAmqpClientEvents();
            _amqpClient.Connect(_messageInterest);
        }

        public void Close()
        {
            _amqpClient.Disconnect();
            DetachAmqpClintEvents();

            SetAsClosed();
        }
    }
}
