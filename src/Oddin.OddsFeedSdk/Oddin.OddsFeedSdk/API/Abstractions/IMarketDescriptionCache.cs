using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface IMarketDescriptionCache
{
    IReadOnlyDictionary<CompositeKey, LocalizedMarketDescription> LocalizedMarketDescriptions(CultureInfo culture);

    LocalizedMarketDescription
        GetMarketDescriptionById(int marketId, string variant, IEnumerable<CultureInfo> cultures);

    LocalizedMarketDescription GetMarketDescription(CompositeKey key);

    void ClearCacheItem(int marketId, string variant);
}