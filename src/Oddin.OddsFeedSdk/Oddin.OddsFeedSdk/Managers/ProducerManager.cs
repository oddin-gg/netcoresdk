using Microsoft.Extensions.Logging;
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
        public const int UNKNOWN_PRODUCER_ID = 99;
        public const int MAX_INACTIVITY_SECONDS = 10;
        public const int STATEFUL_RECOVERY_WINDOW_MINUTES = 60;

        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(ProducerManager));

        private readonly IApiClient _apiClient;
        private readonly ExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IReadOnlyCollection<IProducer> _producers;
        private bool _locked = false;

        public IReadOnlyCollection<IProducer> Producers => _producers;

        public ProducerManager(IApiClient apiClient, IFeedConfiguration configuration)
        {
            if (apiClient is null)
                throw new ArgumentNullException(nameof(apiClient));

            _apiClient = apiClient;
            _exceptionHandlingStrategy = configuration.ExceptionHandlingStrategy;

            try
            {
                _producers = _apiClient
                    .GetProducers()
                    .ToList();
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _exceptionHandlingStrategy);
            }
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
                if (producer.LastTimestampBeforeDisconnect != DateTime.MinValue 
                    && producer.LastTimestampBeforeDisconnect < DateTime.Now.Subtract(TimeSpan.FromMinutes(producer.MaxRecoveryTime)))
                {
                    var err = $"Recovery timestamp for producer {producer.Name} is too far in the past. TimeStamp={producer.LastTimestampBeforeDisconnect}";
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

            var oldestBearableTimestampBeforeDisconnect = DateTime.Now.Subtract(TimeSpan.FromMinutes(producer.MaxRecoveryTime));
            if (timestamp < oldestBearableTimestampBeforeDisconnect)
                throw new ArgumentOutOfRangeException(nameof(timestamp), $"The value {timestamp} specifies the time too far in the past. Timestamp must be greater then {oldestBearableTimestampBeforeDisconnect}");

            producer.SetLastTimestampBeforeDisconnect(timestamp);
        }

        public void RemoveTimestampBeforeDisconnect(int id)
        {
            if (_locked)
                throw new InvalidOperationException("Producers cannot be changed after the feed was opened!");

            if (id <= 0)
                throw new ArgumentException($"Producer id must be a positive integer!");

            var producer = (Producer)Get(id);
            producer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
        }

        public void DisableProducer(int id)
        {
            if (_locked)
                throw new InvalidOperationException("Producers cannot be changed after the feed was opened!");

            var producer = (Producer)Get(id);
            producer.SetDisabled(true);
        }

        private bool TryGet(int id, out IProducer result)
        {
            result = _producers?.FirstOrDefault(p => p.Id == id);
            if (result == default)
            {
                result = CreateUnknownProducer();
                return false;
            }
            return true;
        }

        private bool TryGet(string name, out IProducer result)
        {
            result = _producers?.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if (result == default)
            {
                result = CreateUnknownProducer();
                return false;
            }
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
                UNKNOWN_PRODUCER_ID,
                "Unknown",
                "Unknown producer",
                false,
                "live|prematch",
                MAX_INACTIVITY_SECONDS,
                STATEFUL_RECOVERY_WINDOW_MINUTES);
    }
}
