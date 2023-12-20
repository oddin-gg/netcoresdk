using System;
using System.Collections.Generic;
using System.Linq;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.Managers;

internal class ProducerManager : IProducerManager
{
    private readonly IFeedConfiguration _config;
    private bool _locked;

    public ProducerManager(IApiClient apiClient, IFeedConfiguration config)
    {
        _config = config;
        Producers = apiClient
            .GetProducers()
            .ToList();
    }

    public IReadOnlyCollection<IProducer> Producers { get; }

    void IProducerManager.Lock()
    {
        if (Producers == null)
            throw new CommunicationException("No producer available!");

        if (Producers.Any() == false
            || Producers.Count(c => c.IsAvailable && c.IsDisabled == false) == 0)
        {
            throw new InvalidOperationException("No producer available or all are disabled.");
        }

        foreach (var producer in Producers)
        {
            if (producer.LastProcessedMessageGenTimestamp != 0
                && producer.LastProcessedMessageGenTimestamp <
                Timestamp.Now() - Timestamp.FromMinutes(producer.StatefulRecoveryWindowInMinutes))
            {
                var err =
                    $"Recovery timestamp for producer {producer.Name} is too far in the past. TimeStamp={producer.LastProcessedMessageGenTimestamp}";
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
            throw new ArgumentException("Producer id must be a positive integer!");

        var producer = (Producer)Get(id);

        if (timestamp > DateTime.Now)
            throw new ArgumentOutOfRangeException(nameof(timestamp),
                $"The value {timestamp} specifies the time in the future");

        var oldestBearableTimestampBeforeDisconnect =
            DateTime.Now.Subtract(TimeSpan.FromMinutes(producer.StatefulRecoveryWindowInMinutes));
        if (timestamp < oldestBearableTimestampBeforeDisconnect)
            throw new ArgumentOutOfRangeException(nameof(timestamp),
                $"The value {timestamp} specifies the time too far in the past. Timestamp must be greater then {oldestBearableTimestampBeforeDisconnect}");

        producer.ProducerData.LastAliveReceivedGenTimestamp = timestamp.ToEpochTimeMilliseconds();
    }

    public void RemoveTimestampBeforeDisconnect(int id)
    {
        if (_locked)
            throw new InvalidOperationException("Producers cannot be changed after the feed was opened!");

        if (id <= 0)
            throw new ArgumentException("Producer id must be a positive integer!");

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

    private bool TryGet(int id, out IProducer result)
    {
        var it = Producers.FirstOrDefault(p => p.Id == id);
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
        var it = Producers.FirstOrDefault(p =>
            string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
        if (it == null)
        {
            result = CreateUnknownProducer();
            return false;
        }

        result = it;
        return true;
    }

    private Producer CreateUnknownProducer()
        => new(
            //string apiUrl, string producerScopes, int statefulRecoveryInMinutes)
            new ProducerData(
                SdkDefaults.UnknownProducerId,
                "Unknown",
                "Unknown producer",
                true,
                _config.ApiHost,
                "live|prematch",
                SdkDefaults.StatefulRecoveryWindowInMinutes
            ));
}