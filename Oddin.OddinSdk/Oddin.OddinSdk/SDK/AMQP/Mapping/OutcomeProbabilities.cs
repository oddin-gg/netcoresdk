﻿using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
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
            string outcomeId,
            IApiClient apiClient)
            : base(outcomeId, apiClient)
        {
            Active = active;
            Probabilities = probabilities;
            AdditionalProbabilities = additionalProbabilities;
        }
    }
}