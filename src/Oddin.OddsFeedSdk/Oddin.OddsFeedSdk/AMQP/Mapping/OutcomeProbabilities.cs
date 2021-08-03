using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class OutcomeProbabilities : Outcome, IOutcomeProbabilities
    {
        public bool? Active { get; }

        public double? Probabilities { get; }

        public IAdditionalProbabilities AdditionalProbabilities { get; }

        public OutcomeProbabilities(
            bool? active,
            double? probabilities,
            IAdditionalProbabilities additionalProbabilities,
            long outcomeId,
            long? outcomeRefId,
            IApiClient apiClient)
            : base(outcomeId, outcomeRefId, apiClient)
        {
            Active = active;
            Probabilities = probabilities;
            AdditionalProbabilities = additionalProbabilities;
        }
    }
}
