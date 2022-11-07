using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IProducer
{
    public int Id { get; }

    public string Name { get; }

    public string Description { get; }

    public long LastMessageTimestamp { get; }

    public bool IsAvailable { get; }

    public bool IsDisabled { get; }

    public bool IsProducerDown { get; }

    public string ApiUrl { get; }

    public IEnumerable<ProducerScope> ProducerScopes { get; }

    public long LastProcessedMessageGenTimestamp { get; }

    public long ProcessingQueDelay { get; }

    public long TimestampForRecovery { get; }

    public int StatefulRecoveryWindowInMinutes { get; }

    public IRecoveryInfo RecoveryInfo { get; }
}