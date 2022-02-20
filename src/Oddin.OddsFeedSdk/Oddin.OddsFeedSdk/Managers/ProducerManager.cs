using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oddin.OddsFeedSdk.Managers
{
    internal class ProducerManager : IProducerManager
    {
        private readonly IFeedConfiguration _configuration;
        private readonly IReadOnlyCollection<IProducer> _producers;
        private bool _locked;

        public IReadOnlyCollection<IProducer> Producers => _producers;

        public ProducerManager(IApiClient apiClient, IFeedConfiguration configuration)
        {
            _configuration = configuration;
            _producers = apiClient
                .GetProducers()
                .ToList();
        }

        void IProducerManager.Lock()
        {
            if (_producers == null)
                throw new CommunicationException("No producer available!");

            if (_producers.Any() == false
                || _producers.Count(c => c.IsAvailable && c.IsDisabled == false) == 0)
            {
                throw new InvalidOperationException("No producer available or all are disabled.");
            }

            foreach (var producer in _producers)
            {
                if (producer.LastProcessedMessageGenTimestamp != 0
                    && producer.LastProcessedMessageGenTimestamp < Timestamp.Now() - Timestamp.FromMinutes(producer.StatefulRecoveryWindowInMinutes))
                {
                    var err = $"Recovery timestamp for producer {producer.Name} is too far in the past. TimeStamp={producer.LastProcessedMessageGenTimestamp}";
                    throw new InvalidOperationException(err);
                }
            }

            _locked = true;
        }

        public void AddTimestampBeforeDisconnect(int id, DateTime timestamp)
        {
            if (_locked)
                throw new InvalidOperationException("Producers cannot be changed after the feed was opened!");

            if (id <= 0)
                throw new ArgumentException($"Producer id must be a positive integer!");

            var producer = (Producer)Get(id);

            if (timestamp > DateTime.Now)
                throw new ArgumentOutOfRangeException(nameof(timestamp), $"The value {timestamp} specifies the time in the future");

            var oldestBearableTimestampBeforeDisconnect = DateTime.Now.Subtract(TimeSpan.FromMinutes(producer.StatefulRecoveryWindowInMinutes));
            if (timestamp < oldestBearableTimestampBeforeDisconnect)
                throw new ArgumentOutOfRangeException(nameof(timestamp), $"The value {timestamp} specifies the time too far in the past. Timestamp must be greater then {oldestBearableTimestampBeforeDisconnect}");

            producer.ProducerData.LastAliveReceivedGenTimestamp = timestamp.ToEpochTimeMilliseconds();
        }

        public void RemoveTimestampBeforeDisconnect(int id)
        {
            if (_locked)
                throw new InvalidOperationException("Producers cannot be changed after the feed was opened!");

            if (id <= 0)
                throw new ArgumentException($"Producer id must be a positive integer!");

            var producer = (Producer)Get(id);
            producer.ProducerData.LastAliveReceivedGenTimestamp = 0;
        }

        public void DisableProducer(int id)
        {
            if (_locked)
                throw new InvalidOperationException("Producers cannot be changed after the feed was opened!");

            var producer = (Producer)Get(id);
            producer.ProducerData.Enabled = false;
        }

        private bool TryGet(int id, out IProducer result)
        {
            var it = _producers.FirstOrDefault(p => p.Id == id);
            if (it == null)
            {
                result = CreateUnknownProducer();
                return false;
            }

            result = it;
            return true;
        }

        private bool TryGet(string name, out IProducer result)
        {
            var it = _producers.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if (it == null)
            {
                result = CreateUnknownProducer();
                return false;
            }

            result = it;
            return true;
        }

        public bool Exists(int id)
            => TryGet(id, out var _);

        public bool Exists(string name)
            => TryGet(name, out var _);

        public IProducer Get(int id)
        {
            TryGet(id, out var result);
            return result;
        }

        public IProducer Get(string name)
        {
            TryGet(name, out var result);
            return result;
        }

        private Producer CreateUnknownProducer()
            => new Producer(
                //string apiUrl, string producerScopes, int statefulRecoveryInMinutes)
                new ProducerData(
                    SdkDefaults.UnknownProducerId,
                    "Unknown",
                    "Unknown producer",
                    true,
                    _configuration.ApiHost,
                    "live|prematch",
                    SdkDefaults.StatefulRecoveryWindowInMinutes
                ));
    }
}