using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API
{
    internal class CompetitorCache : ICompetitorCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(CompetitorCache));

        private readonly IApiClient _apiClient;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public CompetitorCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // TODO: Subscribe

        public LocalizedCompetitor GetCompetitor(URN id, IEnumerable<CultureInfo> cultures)
        {
            _semaphore.Wait();
            try
            {
                var alreadyExisting = _cache.Get(id.ToString()) as LocalizedCompetitor;
                var localesExisting = alreadyExisting?.LoadedLocals ?? new List<CultureInfo>();
                var toFetch = localesExisting.Except(cultures);

                if(toFetch.Any())
                {
                    LoadAndCacheItem(id, toFetch);
                }

                return _cache.Get(id.ToString()) as LocalizedCompetitor;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
        {
            foreach (var culture in cultures)
            {
                teamExtended team;
                try
                {
                    team = _apiClient.GetCompetitorProfile(id, culture);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while fetching competitor profile {e}");
                    continue;
                }

                try
                {
                    RefreshOrInsertItem(id, culture, team);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while refreshing competitor {e}");
                }
            }
        }

        private void RefreshOrInsertItem(URN id, CultureInfo culture, teamExtended data)
        {
            if (_cache.Get(id.ToString()) is LocalizedCompetitor item)
            {
                item.IsVirtual = data.virtualSpecified ? data.@virtual : default;
                item.CountryCode = data.country_code;
            }
            else
            {
                item = new LocalizedCompetitor(id)
                {
                    IsVirtual = data.virtualSpecified ? data.@virtual : default,
                    CountryCode = data.country_code
                };
            }
            item.Name[culture] = data.name;

            if (data.abbreviation != null)
            {
                item.Abbreviation[culture] = data.abbreviation;
            }

            if (data.country != null)
            {
                item.Country[culture] = data.country;
            }

            _cache.Set(id.ToString(), item, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(24) });
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }
    }
}
