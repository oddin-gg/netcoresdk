using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;

namespace Oddin.OddsFeedSdk.API
{
    internal class MarketDescriptionFactory : IMarketDescriptionFactory
    {
        private readonly IFeedConfiguration _feedConfiguration;
        private readonly IMarketDescriptionCache _marketDescriptionCache;

        public MarketDescriptionFactory(IFeedConfiguration feedConfiguration, IMarketDescriptionCache marketDescriptionCache)
        {
            _feedConfiguration = feedConfiguration;
            _marketDescriptionCache = marketDescriptionCache;
        }

        public IMarketDescription GetMarketDescription(int marketId, IDictionary<string, string> specifiers, IEnumerable<CultureInfo> cultures)
        {
            return new MarketDescription(
                marketId,
                specifiers?.FirstOrDefault(s => s.Key == "variant").Value,
                _marketDescriptionCache,
                _feedConfiguration.ExceptionHandlingStrategy,
                cultures.ToHashSet());
        }

        public IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture)
        {
            var keys = _marketDescriptionCache.GetMarketDescriptions(culture);

            return keys.Select(k
                => new MarketDescription(
                    k.MarketId,
                    k.Variant,
                    _marketDescriptionCache,
                    _feedConfiguration.ExceptionHandlingStrategy,
                    new[] { culture }));
        }
    }
}
