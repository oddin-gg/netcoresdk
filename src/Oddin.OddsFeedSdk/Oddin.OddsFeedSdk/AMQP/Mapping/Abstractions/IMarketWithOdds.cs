using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IMarketWithOdds : IMarket
    {
        IEnumerable<IOutcomeOdds> OutcomeOdds { get; }

        IMarketMetadata MarketMetadata { get; }
    }
}
