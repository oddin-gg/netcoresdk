using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    public interface IMarketDescriptionManager
    {
        IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture = null);

        void DeleteVariantMarketDescriptionFromCache(int marketId, string variantValue);
    }
}
