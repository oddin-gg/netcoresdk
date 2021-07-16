﻿using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.Dispatch.EventArguments;
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

        public event EventHandler<FeedCloseEventArgs> Closed;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

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

            foreach (var prm in _producerRecoveryManagers)
            {
                prm.Closed += OnClosed;
                prm.ProducerDown += OnProducerDown;
                prm.ProducerUp += OnProducerUp;
            }
        }

        private void DetachFromEvents()
        {
            _amqpClient.SnapshotCompleteMessageReceived -= OnSnapshotCompleteReceived;

            foreach (var prm in _producerRecoveryManagers)
            {
                prm.Closed -= OnClosed;
                prm.ProducerDown -= OnProducerDown;
                prm.ProducerUp -= OnProducerUp;
            }
        }

        public void Open()
        {
            GenerateProducerRecoveryManagers();
            AttachToEvents();

            foreach (var producerRecoveryManager in _producerRecoveryManagers)
                producerRecoveryManager.Open();
        }

        public void Close()
        {
            foreach (var producerRecoveryManager in _producerRecoveryManagers)
                producerRecoveryManager.Close();

            DetachFromEvents();
        }

        private bool TryGetProducerRecoveryManager(int producerId, out ProducerRecoveryManager prm)
        {
            var prms = _producerRecoveryManagers.Where(prm => prm.MatchesProducer(producerId));
            if (prms.Count() >= 2)
            {
                var errorMessage = $"There are multiple {typeof(ProducerRecoveryManager).Name}s related to the same Producer ID!";
                _log.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            prm = prms.FirstOrDefault();
            return prm != default;
        }

        private void OnSnapshotCompleteReceived(object sender, SimpleMessageEventArgs<snapshot_complete> eventArgs)
        {
            var producerId = eventArgs?.FeedMessage?.product;
            if (producerId.HasValue == false)
            {
                _log.LogWarning($"An incomplete {typeof(snapshot_complete).Name} message was received!");
                return;
            }

            if (TryGetProducerRecoveryManager(producerId.Value, out var prm) == false)
                return;

            prm.HandleSnapshotCompletedReceived(eventArgs.FeedMessage);
        }

        private void OnAliveReceived(object sender, SimpleMessageEventArgs<alive> eventArgs)
        {
            var producerId = eventArgs?.FeedMessage?.product;
            if (producerId.HasValue == false)
            {
                _log.LogWarning($"An incomplete {typeof(alive).Name} message was received!");
                return;
            }

            if (TryGetProducerRecoveryManager(producerId.Value, out var prm) == false)
                return;

            prm.HandleAliveReceived(eventArgs.FeedMessage);
        }

        private void OnClosed(object sender, FeedCloseEventArgs eventArgs)
        {
            Dispatch(Closed, eventArgs, nameof(Closed));
        }

        private void OnProducerDown(object sender, ProducerStatusChangeEventArgs eventArgs)
        {
            Dispatch(ProducerDown, eventArgs, nameof(ProducerDown));
        }

        private void OnProducerUp(object sender, ProducerStatusChangeEventArgs eventArgs)
        {
            Dispatch(ProducerUp, eventArgs, nameof(ProducerUp));
        }
    }
}
