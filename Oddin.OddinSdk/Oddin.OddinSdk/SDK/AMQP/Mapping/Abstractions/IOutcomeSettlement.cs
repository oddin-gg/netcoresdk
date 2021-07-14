using Oddin.OddinSdk.SDK.AMQP.Enums;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    public interface IOutcomeSettlement : IOutcome
    {
        double? DeadHeatFactor { get; }

        VoidFactor? VoidFactor { get; }

        OutcomeResult OutcomeResult { get; }
    }
}