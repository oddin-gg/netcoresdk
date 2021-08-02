using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API
{
    internal class FixtureCache : IFixtureCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(TournamentsCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new MemoryCache(nameof(FixtureCache));

        public FixtureCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public LocalizedFixture GetFixture(URN id, CultureInfo culture)
        {
            var item = _cache.Get(id.ToString());
            if (item == null)
                LoadAndCacheItem(id, culture);

            return _cache.Get(id.ToString()) as LocalizedFixture;
        }

        internal void LoadAndCacheItem(URN id, CultureInfo culture)
        {

            FixturesEndpointModel fixtureData;
            try
            {
                fixtureData = _apiClient.GetFixture(id, culture);
            }
            catch (Exception e)
            {
                _log.LogError($"Error while fetching fixture {culture.TwoLetterISOLanguageName}: {e}");
                return;
            }

            var item = new LocalizedFixture(
                fixtureData.fixture.start_timeSpecified ? fixtureData.fixture.start_time : default(DateTime?),
                fixtureData.fixture.extra_info?.ToDictionary(i => i.key, i => i.value),
                fixtureData.fixture.tv_channels?.Select(t =>
                    new TvChannel(
                        t.name,
                        t.start_timeSpecified ? t.start_time : default(DateTime?),
                        t.stream_url)
                    )
                );

            _cache.Set(id.ToString(), item, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(12) });
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }
    }
}