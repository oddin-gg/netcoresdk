using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API
{
    internal class MarketDescriptionManager : IMarketDescriptionManager
    {
        public MarketDescriptionManager()
        {
        }

        public IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture = null) => throw new NotImplementedException();

        public void DeleteVariantMarketDescriptionFromCache(int marketId, string variantValue) => throw new NotImplementedException();
    }
}
