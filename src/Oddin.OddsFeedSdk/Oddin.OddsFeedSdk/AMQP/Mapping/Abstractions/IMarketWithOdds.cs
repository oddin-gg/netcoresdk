using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IMarketWithOdds : IMarket
{
    MarketStatus Status { get; }

    bool IsFavorite { get; }

    IEnumerable<IOutcomeOdds> OutcomeOdds { get; }

    IMarketMetadata MarketMetadata { get; }
}