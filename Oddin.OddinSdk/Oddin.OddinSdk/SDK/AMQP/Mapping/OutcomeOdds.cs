using Oddin.OddinSdk.SDK.AMQP.Enums;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Abstractions;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping
{
    internal class OutcomeOdds : OutcomeProbabilities, IOutcomeOdds
    {
        private readonly double? _odds;

        public OutcomeOdds(
            double? odds,
            bool? active,
            double? probabilities,
            IAdditionalProbabilities additionalProbabilities,
            string outcomeId,
            IApiClient apiClient)
            : base(active, probabilities, additionalProbabilities, outcomeId, apiClient)
        {
            _odds = odds;
        }

        private decimal? ConvertToUsOdds(decimal odds)
        {
            decimal? result;

            if (odds == 1)
                result = null;
            else if (odds >= 2)
                result = (odds - 1) * 100;
            else
                result = -100 / (odds - 1);

            return result;
        }

        public double? GetOdds(OddsDisplayType oddsDisplayType = OddsDisplayType.Decimal)
        {
            if (oddsDisplayType == OddsDisplayType.Decimal)
                return _odds;

            return (double?)ConvertToUsOdds((decimal)_odds);
        }
    }
}
