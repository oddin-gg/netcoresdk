using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API;

internal class MarketVoidReasonsCache : IMarketVoidReasonsCache
{
    private const string MemoryCacheKey = "MarketVoidReasons";
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

    private readonly IApiClient _apiClient;
    private readonly MemoryCache _cache = new(nameof(MarketVoidReasonsCache));
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(24);
    private readonly object _lock = new();

    public MarketVoidReasonsCache(IApiClient apiClient) => _apiClient = apiClient;

    public IEnumerable<IMarketVoidReason> GetMarketVoidReasons()
    {
        lock (_lock)
        {
            var voidReasons = _cache.Get(MemoryCacheKey) as IEnumerable<IMarketVoidReason>;
            if (voidReasons == null)
            {
                voidReasons = LoadItems();
                _cache.Set(MemoryCacheKey, voidReasons, _cacheTtl.AsCachePolicy());
            }

            return voidReasons;
        }
    }

    private IEnumerable<IMarketVoidReason> LoadItems()
    {
        MarketVoidReasonsModel marketVoidReasonsModel;
        try
        {
            marketVoidReasonsModel = _apiClient
                .GetMarketVoidReasonsAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception e)
        {
            _log.LogError($"Unable to get void reasons descriptions from api {e}");
            return null;
        }

        var result = new List<IMarketVoidReason>();

        foreach (var void_reason in marketVoidReasonsModel.void_reasons)
        {
            var voidReasonParams = new List<string>();
            if (void_reason.void_reason_params != null)
            {
                voidReasonParams = void_reason.void_reason_params.Select(param => param.name).ToList();
            }

            var voidReason = new MarketVoidReason(
                void_reason.id,
                void_reason.name,
                void_reason.description,
                void_reason.template,
                voidReasonParams
            );

            result.Add(voidReason);
        }

        return result;
    }
}