using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.AMQP.Mapping
{
    internal class MarketWithOdds : Market, IMarketWithOdds
    {
        public MarketStatus Status { get; }

        public bool IsFavorite { get; }

        public IEnumerable<IOutcomeOdds> OutcomeOdds { get; }

        public IMarketMetadata MarketMetadata { get; }

        public MarketWithOdds(
            int marketId,
            int refId,
            IDictionary<string, string> specifiers,
            string extendedSpecifiers,
            IEnumerable<string> groups,
            IMarketDescriptionFactory marketDescriptionFactory,
            ExceptionHandlingStrategy exceptionHandlingStrategy,
            MarketStatus marketStatus,
            bool isFavorite,
            IEnumerable<IOutcomeOdds> outcomeOdds,
            IMarketMetadata marketMetadata)
            : base(marketId, refId, specifiers, extendedSpecifiers, groups, marketDescriptionFactory, exceptionHandlingStrategy)
        {
            Status = marketStatus;
            IsFavorite = isFavorite;
            OutcomeOdds = outcomeOdds;
            MarketMetadata = marketMetadata;
        }
    }
}
