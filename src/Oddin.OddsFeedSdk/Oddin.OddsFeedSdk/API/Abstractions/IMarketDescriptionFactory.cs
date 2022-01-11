using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IMarketDescriptionFactory
    {
        IMarketDescription GetMarketDescription(int marketId, IReadOnlyDictionary<string, string> specifiers, IEnumerable<CultureInfo> cultures);

        IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture);
    }
}