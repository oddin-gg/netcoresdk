using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

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
            long outcomeRefId,
            IMarketDescriptionManager marketDescriptionManager)
            : base(outcomeId, outcomeRefId, marketDescriptionManager)
        {
            Active = active;
            Probabilities = probabilities;
            AdditionalProbabilities = additionalProbabilities;
        }
    }
}
