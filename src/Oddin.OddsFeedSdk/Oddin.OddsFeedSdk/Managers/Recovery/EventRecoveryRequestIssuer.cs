using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.Managers.Recovery
{
    internal class EventRecoveryRequestIssuer : DispatcherBase, IEventRecoveryRequestIssuer, IEventRecoveryCompletedDispatcher
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(EventRecoveryRequestIssuer));

        private readonly IApiClient _apiClient;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IRequestIdFactory _requestIdFactory;
        private readonly IAmqpClient _amqpClient;
        private readonly Dictionary<long, URN> _pendingRequests = new Dictionary<long, URN>();
        private bool _isOpen = false;

        public EventRecoveryRequestIssuer(IApiClient apiClient, IRequestIdFactory requestIdFactory, IAmqpClient amqpClient, IFeedConfiguration config)
        {
            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (requestIdFactory is null)
                throw new ArgumentNullException(nameof(requestIdFactory));

            if (amqpClient is null)
                throw new ArgumentNullException(nameof(amqpClient));

            if (config is null)
                throw new ArgumentNullException(nameof(config));

            _apiClient = apiClient;
            _requestIdFactory = requestIdFactory;
            _amqpClient = amqpClient;
            _exceptionHandlingStrategy = config.ExceptionHandlingStrategy;
        }

        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        private void OnSnapshotCompleteReceived(object sender, SimpleMessageEventArgs<snapshot_complete> eventArgs)
        {
            var requestId = eventArgs?.FeedMessage?.request_id;
            if (requestId.HasValue == false)
            {
                _log.LogWarning($"A {typeof(snapshot_complete).Name} message without request ID was received!");
                return;
            }

            if (_pendingRequests.ContainsKey(requestId.Value))
            {
                var urn = _pendingRequests[requestId.Value];
                var ercEventArgs = new EventRecoveryCompletedEventArgs(requestId.Value, urn);
                Dispatch(EventRecoveryCompleted, ercEventArgs, nameof(EventRecoveryCompleted));
                _pendingRequests.Remove(requestId.Value);
            }
        }

        private async Task<long> RecoverMessage(IProducer producer, URN eventId, Func<string, URN, long, Task<long>> apiCall)
        {
            if (_isOpen == false)
                throw new InvalidOperationException($"Cannot request event recovery on {typeof(IEventRecoveryRequestIssuer)} that hasn't been opened yet!");

            try
            {
                var requestId = _requestIdFactory.GetNext();
                if (_pendingRequests.ContainsKey(requestId))
                    throw new SdkException($"Recover request of event {eventId} could not be created, most likely because there are too many recover requests (recover request ID {requestId} already exists)!");

                _pendingRequests[requestId] = eventId;

                return await apiCall(producer.Name, eventId, requestId);
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
            return default;
        }

        public async Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId)
            => await RecoverMessage(producer, eventId, _apiClient.PostEventRecoveryRequest);

        public async Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId)
            => await RecoverMessage(producer, eventId, _apiClient.PostEventStatefulRecoveryRequest);

        public void Open()
        {
            _amqpClient.SnapshotCompleteMessageReceived += OnSnapshotCompleteReceived;

            _isOpen = true;
        }

        public void Close()
        {
            _amqpClient.SnapshotCompleteMessageReceived -= OnSnapshotCompleteReceived;

            _isOpen = false;
        }
    }
}
