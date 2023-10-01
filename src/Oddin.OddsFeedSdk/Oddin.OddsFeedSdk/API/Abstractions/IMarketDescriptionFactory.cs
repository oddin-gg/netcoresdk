using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface IMarketDescriptionFactory
{
    IPlayerCache PlayerCache { get; }

    ICompetitorCache CompetitorCache { get; }

    IMarketDescription MarketDescriptionByIdAndSpecifiers(int marketId, IReadOnlyDictionary<string, string> specifiers,
        IEnumerable<CultureInfo> cultures);

    IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture);

    IMarketDescription GetMarketDescriptionByIdAndVariant(int marketId, string? variant,
        IEnumerable<CultureInfo> cultures);

    IEnumerable<IMarketVoidReason> GetMarketVoidReasons();
}