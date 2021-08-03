using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.Managers.Recovery
{
    internal class FeedRecoveryManager : DispatcherBase, IFeedRecoveryManager
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(FeedRecoveryManager));
        private readonly IFeedConfiguration _config;
        private readonly IProducerManager _producerManager;
        private readonly IApiClient _apiClient;
        private readonly IRequestIdFactory _requestIdFactory;
        private readonly IAmqpClient _amqpClient;
        private IEnumerable<ProducerRecoveryManager> _producerRecoveryManagers;
        private EventHandler<SimpleMessageEventArgs<alive>> _onAliveReceivedDelegate;

        public event EventHandler<FeedCloseEventArgs> Closed;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        public FeedRecoveryManager(IFeedConfiguration config, IProducerManager producerManager, IApiClient apiClient, IRequestIdFactory requestIdFactory, IAmqpClient amqpClient)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            if (producerManager is null)
                throw new ArgumentNullException(nameof(producerManager));

            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            if (requestIdFactory is null)
                throw new ArgumentNullException(nameof(requestIdFactory));

            if (amqpClient is null)
                throw new ArgumentNullException(nameof(amqpClient));

            _config = config;
            _producerManager = producerManager;
            _apiClient = apiClient;
            _requestIdFactory = requestIdFactory;
            _amqpClient = amqpClient;

            _onAliveReceivedDelegate = async (sender, eventArgs) => await OnAliveReceived(sender, eventArgs);
        }

        private void GenerateProducerRecoveryManagers()
        {
            var result = new List<ProducerRecoveryManager>();
            foreach (var p in _producerManager.Producers)
            {
                if (p.IsAvailable == false
                    || p.IsDisabled)
                    continue;

                result.Add(new ProducerRecoveryManager(_config, p, _apiClient, _requestIdFactory));
            }
            _producerRecoveryManagers = result;
        }

        private void AttachToEvents()
        {
            _amqpClient.AliveMessageReceived += _onAliveReceivedDelegate;
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
            _amqpClient.AliveMessageReceived -= _onAliveReceivedDelegate;
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

        private async Task OnAliveReceived(object sender, SimpleMessageEventArgs<alive> eventArgs)
        {
            var producerId = eventArgs?.FeedMessage?.product;
            if (producerId.HasValue == false)
            {
                _log.LogWarning($"An incomplete {typeof(alive).Name} message was received!");
                return;
            }

            if (TryGetProducerRecoveryManager(producerId.Value, out var prm) == false)
                return;

            await prm.HandleAliveReceived(eventArgs.FeedMessage);
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
