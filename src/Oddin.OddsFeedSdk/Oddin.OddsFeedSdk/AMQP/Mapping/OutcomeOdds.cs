using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.Mapping;

internal class OutcomeOdds : OutcomeProbabilities, IOutcomeOdds
{
    private readonly double? _odds;

    public OutcomeOdds(
        double? odds,
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
        : base(
            active,
            probabilities,
            additionalProbabilities,
            outcomeId,
            outcomeRefId,
            marketDescriptionFactory,
            configuration,
            marketId,
            marketSpecifiers,
            sportEvent) =>
        _odds = odds;

    public double? GetOdds(OddsDisplayType oddsDisplayType = OddsDisplayType.Decimal)
    {
        if (oddsDisplayType == OddsDisplayType.Decimal)
            return _odds;

        return (double?)ConvertToUsOdds((decimal)_odds);
    }

    private decimal? ConvertToUsOdds(decimal odds)
    {
        decimal? result;

        if (odds == 1)
            result = null;
        else if (odds >= 2)
            result = ( odds - 1 ) * 100;
        else
            result = -100 / ( odds - 1 );

        return result;
    }
}