using Oddin.OddsFeedSdk.AMQP.Enums;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IOutcomeSettlement : IOutcome
{
    double? DeadHeatFactor { get; }

    VoidFactor? VoidFactor { get; }

    OutcomeResult OutcomeResult { get; }
}