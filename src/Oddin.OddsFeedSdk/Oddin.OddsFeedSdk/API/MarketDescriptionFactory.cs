using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API;

internal class MarketDescriptionFactory : IMarketDescriptionFactory
{
    private readonly IFeedConfiguration _feedConfiguration;
    private readonly IMarketDescriptionCache _marketDescriptionCache;
    private readonly IMarketVoidReasonsCache _marketVoidReasonsCache;

    public MarketDescriptionFactory(
        IFeedConfiguration feedConfiguration,
        IMarketDescriptionCache marketDescriptionCache,
        IMarketVoidReasonsCache marketVoidReasonsCache,
        IPlayerCache playerCache,
        ICompetitorCache competitorCache
    )
    {
        _feedConfiguration = feedConfiguration;
        _marketDescriptionCache = marketDescriptionCache;
        _marketVoidReasonsCache = marketVoidReasonsCache;
        PlayerCache = playerCache;
        CompetitorCache = competitorCache;
    }

    public IMarketDescription MarketDescriptionByIdAndSpecifiers(int marketId,
        IReadOnlyDictionary<string, string> specifiers,
        IEnumerable<CultureInfo> cultures) =>
        GetMarketDescriptionByIdAndVariant(marketId, specifiers?.FirstOrDefault(s => s.Key == "variant").Value,
            cultures);

    public IMarketDescription GetMarketDescriptionByIdAndVariant(int marketId, string? variant,
        IEnumerable<CultureInfo> cultures)
    {
        var mds = _marketDescriptionCache.GetMarketDescriptionById(marketId, variant, cultures);

        if (mds == null)
        {
            if (_feedConfiguration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
            {
                throw new ItemNotFoundException(marketId.ToString(), "Market description not found");
            }

            return null;
        }

        return new MarketDescription(
            marketId,
            mds.IncludesOutcomesOfType,
            mds.OutcomeType,
            variant,
            _marketDescriptionCache,
            _feedConfiguration.ExceptionHandlingStrategy,
            cultures.ToHashSet());
    }

    public IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture)
    {
        var marketDescriptions = _marketDescriptionCache.LocalizedMarketDescriptions(culture);

        return marketDescriptions.Select(o
            => new MarketDescription(
                o.Key.MarketId,
                o.Value.IncludesOutcomesOfType,
                o.Value.OutcomeType,
                o.Key.Variant,
                _marketDescriptionCache,
                _feedConfiguration.ExceptionHandlingStrategy,
                new[] { culture }));
    }

    public IEnumerable<IMarketVoidReason> GetMarketVoidReasons() => _marketVoidReasonsCache.GetMarketVoidReasons();

    public IPlayerCache PlayerCache { get; }

    public ICompetitorCache CompetitorCache { get; }
}