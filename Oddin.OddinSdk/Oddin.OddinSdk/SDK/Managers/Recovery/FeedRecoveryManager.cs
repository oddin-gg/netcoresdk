using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oddin.OddinSdk.SDK.Managers.Recovery
{
    internal class FeedRecoveryManager : DispatcherBase, IFeedRecoveryManager
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(FeedRecoveryManager));
        private readonly IProducerManager _producerManager;
        private readonly IApiClient _apiClient;
        private readonly IRequestIdFactory _requestIdFactory;
        private readonly IAmqpClient _amqpClient;
        private IEnumerable<ProducerRecoveryManager> _producerRecoveryManagers;

        public FeedRecoveryManager(IProducerManager producerManager, IApiClient apiClient, IRequestIdFactory requestIdFactory, IAmqpClient amqpClient)
        {
            if (producerManager is null)
                throw new ArgumentNullException(nameof(producerManager));

            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (requestIdFactory is null)
                throw new ArgumentNullException(nameof(requestIdFactory));

            if (amqpClient is null)
                throw new ArgumentNullException(nameof(amqpClient));

            _producerManager = producerManager;
            _apiClient = apiClient;
            _requestIdFactory = requestIdFactory;
            _amqpClient = amqpClient;
        }

        private void GenerateProducerRecoveryManagers()
        {
            _producerRecoveryManagers = _producerManager.Producers
                .Where(p => p.IsAvailable)
                .Where(p => p.IsDisabled == false)
                .Select(p => new ProducerRecoveryManager(p, _apiClient, _requestIdFactory));
        }

        private void AttachToEvents()
        {
            _amqpClient.SnapshotCompleteMessageReceived += OnSnapshotCompleteReceived;
        }

        private void DetachFromEvents()
        {
            _amqpClient.SnapshotCompleteMessageReceived -= OnSnapshotCompleteReceived;
        }

        public void Open()
        {
            AttachToEvents();
            GenerateProducerRecoveryManagers();

            //foreach (var producerRecoveryManager in _producerRecoveryManagers)
            //    producerRecoveryManager.Open();
        }

        public void Close()
        {
            //foreach (var producerRecoveryManager in _producerRecoveryManagers)
            //    producerRecoveryManager.Close();

            DetachFromEvents();
        }

        private void OnSnapshotCompleteReceived(object sender, SimpleMessageEventArgs<snapshot_complete> eventArgs)
        {
            var requestId = eventArgs?.FeedMessage?.request_id;
            if (requestId.HasValue == false)
            {
                _log.LogWarning($"A {typeof(snapshot_complete).Name} message without request ID was received!");
                return;
            }
            
            // TODO
        }
    }
}
