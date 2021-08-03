using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IMarketDescriptionFactory
    {
        IMarketDescription GetMarketDescription(int marketId, IDictionary<string, string> specifiers, IEnumerable<CultureInfo> cultures);

        IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture);
    }
}
