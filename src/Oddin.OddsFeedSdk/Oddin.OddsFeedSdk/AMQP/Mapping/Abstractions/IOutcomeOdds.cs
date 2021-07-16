using Oddin.OddsFeedSdk.AMQP.Enums;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IOutcomeOdds : IOutcomeProbabilities
    {
        double? GetOdds(OddsDisplayType oddsDisplayType = OddsDisplayType.Decimal);
    }
}
