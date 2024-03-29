using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers.Abstractions;

namespace Oddin.OddsFeedSdk.Managers;

internal class MarketDescriptionManager : IMarketDescriptionManager
{
    private readonly ICacheManager _cacheManager;
    private readonly IExceptionWrapper _exceptionWrapper;
    private readonly IFeedConfiguration _feedConfiguration;
    private readonly IMarketDescriptionFactory _marketDescriptionFactory;

    public MarketDescriptionManager(
        IFeedConfiguration feedConfiguration,
        IMarketDescriptionFactory marketDescriptionFactory,
        ICacheManager cacheManager,
        IExceptionWrapper exceptionWrapper)
    {
        _feedConfiguration = feedConfiguration;
        _marketDescriptionFactory = marketDescriptionFactory;
        _cacheManager = cacheManager;
        _exceptionWrapper = exceptionWrapper;
    }

    public IEnumerable<IMarketDescription> GetMarketDescriptions(CultureInfo culture = null)
    {
        if (culture is null)
            culture = _feedConfiguration.DefaultLocale;

        return _exceptionWrapper.Wrap(()
            => _marketDescriptionFactory.GetMarketDescriptions(culture));
    }

    public IMarketDescription GetMarketDescriptionByIdAndVariant(int marketId, string? variant)
    {
        var locale = _feedConfiguration.DefaultLocale;
        return _marketDescriptionFactory.GetMarketDescriptionByIdAndVariant(marketId, variant, new[] { locale });
    }

    public IEnumerable<IMarketVoidReason> GetMarketVoidReasons() =>
        _exceptionWrapper.Wrap(()
            => _marketDescriptionFactory.GetMarketVoidReasons());

    public void DeleteVariantMarketDescriptionFromCache(int marketId, string variantValue)
        => _cacheManager.MarketDescriptionCache.ClearCacheItem(marketId, variantValue);
}