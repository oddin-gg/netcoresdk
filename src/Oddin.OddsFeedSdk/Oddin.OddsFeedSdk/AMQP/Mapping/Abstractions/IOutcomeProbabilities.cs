namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IOutcomeProbabilities : IOutcome
{
    bool? Active { get; }

    double? Probabilities { get; }

    IAdditionalProbabilities AdditionalProbabilities => null;
}