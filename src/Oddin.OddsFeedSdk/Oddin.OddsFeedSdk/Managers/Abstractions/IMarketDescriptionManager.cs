using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.Managers.Abstractions;

public interface IMarketDescriptionManager
{
    IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture = null);

    IMarketDescription GetMarketDescriptionByIdAndVariant(int marketId, string? variant);

    void DeleteVariantMarketDescriptionFromCache(int marketId, string variantValue);

    // Get MarketVoidReasons from cache
    IEnumerable<IMarketVoidReason> GetMarketVoidReasons();
}