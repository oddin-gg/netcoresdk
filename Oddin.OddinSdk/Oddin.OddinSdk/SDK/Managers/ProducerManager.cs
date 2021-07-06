﻿using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oddin.OddinSdk.SDK.Managers
{
    internal class ProducerManager : LoggingBase, IProducerManager
    {
        public const int UNKNOWN_PRODUCER_ID = 99;
        public const int STATEFUL_RECOVERY_WINDOW_MINUTES = 60;

        private readonly IApiClient _apiClient;
        private IReadOnlyCollection<IProducer> _producers;

        public IReadOnlyCollection<IProducer> Producers => _producers;

        public void AddTimestampBeforeDisconnect(int id, DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public void DisableProducer(int id)
        {
            throw new NotImplementedException();
        }

        private bool TryGet(int id, out IProducer result)
        {
            result = _producers.FirstOrDefault(p => p.Id == id);
            if (result == default)
            {
                result = CreateUnknownProducer();
                return false;
            }
            return true;
        }

        private bool TryGet(string name, out IProducer result)
        {
            result = _producers.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
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

        public void RemoveTimestampBeforeDisconnect(int id)
        {
            throw new NotImplementedException();
        }


        public ProducerManager(IApiClient apiClient, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _apiClient = apiClient;
            _producers = _apiClient.GetProducers();
        }


        private Producer CreateUnknownProducer()
            => new Producer(UNKNOWN_PRODUCER_ID, "Unknown", "Unknown producer", false, "live|prematch", STATEFUL_RECOVERY_WINDOW_MINUTES);
    }
}
