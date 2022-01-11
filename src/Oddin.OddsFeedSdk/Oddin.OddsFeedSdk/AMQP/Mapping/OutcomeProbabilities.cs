using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
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
            IMarketDescriptionFactory marketDescriptionFactory,
            IFeedConfiguration configuration,
            int marketId,
            IReadOnlyDictionary<string, string> marketSpecifiers,
            ISportEvent sportEvent)
            : base(outcomeId, outcomeRefId, marketDescriptionFactory, configuration, marketId, marketSpecifiers, sportEvent)
        {
            Active = active;
            Probabilities = probabilities;
            AdditionalProbabilities = additionalProbabilities;
        }
    }
}