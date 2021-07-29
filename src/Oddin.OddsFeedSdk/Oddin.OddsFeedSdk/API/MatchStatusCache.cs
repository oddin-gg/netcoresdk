using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API
{
    internal class MatchStatusCache : IMatchStatusCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(MatchStatusCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache = new MemoryCache(nameof(MatchStatusCache));

        private readonly Semaphore _semaphore = new Semaphore(1, 1);

        public MatchStatusCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // TODO: Subscribe + dispose to events from api

        public LocalizedMatchStatus GetMatchStatus(URN id)
        {
            _semaphore.WaitOne();
            try
            {
                var matchStatus = _cache.Get(id.ToString()) as LocalizedMatchStatus;
                if(matchStatus == null)
                {
                    _apiClient.GetMatchSummary(id, Feed.AvailableLanguages().First());
                    matchStatus = _cache.Get(id.ToString()) as LocalizedMatchStatus;
                }
                return matchStatus;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        //internal void RefreshOrInsertItem(URN id, CultureInfo culture, ...model...)
        //{
        //  // TODO
        //}

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }
    }
}
