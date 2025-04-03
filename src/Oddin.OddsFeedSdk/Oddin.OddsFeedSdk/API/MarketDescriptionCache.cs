using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API;

internal class MarketDescriptionCache : IMarketDescriptionCache
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(MarketDescriptionCache));
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(24);

    private readonly HashSet<CultureInfo> _loadedLocals = new();
    private readonly object _lock = new();

    public MarketDescriptionCache(IApiClient apiClient) => _apiClient = apiClient;

    public IReadOnlyDictionary<CompositeKey, LocalizedMarketDescription> LocalizedMarketDescriptions(
        CultureInfo culture)
    {
        lock (_lock)
        {
            if (_loadedLocals.TryGetValue(culture, out _) == false)
                LoadAndCacheItem(0, null, new[] { culture });

            var items = _cache.Where(c =>
            {
                return c.Value is LocalizedMarketDescription localizedItem &&
                       localizedItem.LoadedLocals.Any(i => i.Equals(culture));
            });

            var result = new Dictionary<CompositeKey, LocalizedMarketDescription>();

            foreach (var o in items)
            {
                if (CompositeKey.TryParse(o.Key, out var key))
                {
                    result[key] = o.Value as LocalizedMarketDescription;
                }
            }

            return result;
        }
    }

    public LocalizedMarketDescription GetMarketDescriptionById(int marketId, string variant,
        IEnumerable<CultureInfo> cultures)
    {
        lock (_lock)
        {
            var key = new CompositeKey(marketId, variant);
            var localizedMarketDescription = _cache.Get(key.Key) as LocalizedMarketDescription;
            var culturesSet = localizedMarketDescription?.LoadedLocals ?? new List<CultureInfo>();

            var toFetch = cultures.Except(culturesSet);
            if (toFetch.Any())
                LoadAndCacheItem(marketId, variant, toFetch);

            return _cache.Get(key.Key) as LocalizedMarketDescription;
        }
    }

    public LocalizedMarketDescription GetMarketDescription(CompositeKey key)
        => _cache.Get(key.ToString()) as LocalizedMarketDescription;

    public void ClearCacheItem(int marketId, string variant)
        => _cache.Remove(new CompositeKey(marketId, variant).ToString());

    private void LoadAndCacheItem(
        int marketId,
        string variant,
        IEnumerable<CultureInfo> cultures
    )
    {
        foreach (var culture in cultures)
        {
            MarketDescriptionsModel marketDescriptions;
            try
            {
                if (marketId > 0 && variant != null && isMarketVariantWithDynamicOutcomes(variant))
                {
                    marketDescriptions = _apiClient
                        .GetMarketDescriptionsWithDynamicOutcomesAsync(marketId, variant, culture)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    marketDescriptions = _apiClient
                        .GetMarketDescriptionsAsync(culture)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Unable to get market descriptions from api for culture {culture} {e}");
                continue;
            }

            foreach (var description in marketDescriptions.market)
            {
                try
                {
                    RefreshOrInsertItem(description, culture);
                }
                catch (Exception e)
                {
                    _log.LogError($"Unable to refresh market descriptions in cache for culture {culture} {e}");
                }

                _loadedLocals.Add(culture);
            }
        }
    }

    private void RefreshOrInsertItem(market_description marketDescription, CultureInfo culture)
    {
        var key = new CompositeKey(marketDescription.id, marketDescription.variant);
        var item = _cache.Get(key.Key) as LocalizedMarketDescription;
        var specifiers = marketDescription.specifiers?.Select(s => new Specifier(s.name, s.type));
        var groups = marketDescription.groups?.Split('|') ?? Array.Empty<string>();

        if (item == null)
        {
            var outcomes = marketDescription.outcomes.ToDictionary(o => o.id, o => new LocalizedOutcomeDescription());

            item = new LocalizedMarketDescription(
                marketDescription.refid,
                outcomes,
                marketDescription.includes_outcomes_of_type,
                marketDescription.outcome_type,
                groups
            );
        }

        foreach (var outcome in marketDescription.outcomes)
        {
            var localizedDescription = item.Outcomes.FirstOrDefault(i => i.Key == outcome.id).Value;
            if (localizedDescription == null)
                localizedDescription = item.Outcomes[outcome.id] = new LocalizedOutcomeDescription();

            localizedDescription.Name[culture] = outcome.name;
            localizedDescription.RefId = outcome.refid;

            if (outcome.description != null)
                localizedDescription.Description[culture] = outcome.description;
        }

        item.Name[culture] = marketDescription.name;
        item.Specifiers = specifiers;

        _cache.Set(key.Key, item, _cacheTtl.AsCachePolicy());
    }

    private bool isMarketVariantWithDynamicOutcomes(string marketVariant) =>
        marketVariant.StartsWith("od:dynamic_outcomes:");
}