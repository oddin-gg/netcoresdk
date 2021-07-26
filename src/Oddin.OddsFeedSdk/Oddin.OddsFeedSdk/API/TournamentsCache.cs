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
    internal interface ITournamentsCache
    {
        void ClearCacheItem(URN id);
        LocalizedTournament GetTournament(URN id, IEnumerable<CultureInfo> cultures);
    }

    internal class TournamentsCache : ITournamentsCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(TournamentsCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TournamentsCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _cache = MemoryCache.Default;
        }

        // TODO: Subscribe

        public LocalizedTournament GetTournament(URN id, IEnumerable<CultureInfo> cultures)
        {
            _semaphore.Wait();
            try
            {
                var localizedTournament = _cache.Get(id.ToString()) as LocalizedTournament;
                var localizedAlready = localizedTournament?.LoadedLocals ?? new List<CultureInfo>();

                var culturesToLoad = cultures.Except(localizedAlready);
                if (culturesToLoad.Any())
                    LoadAndCacheItem(id, culturesToLoad);

                return _cache.Get(id.ToString()) as LocalizedTournament;
            }
            finally
            {
                _semaphore.Release();
            }
        }


        internal void LoadAndCacheItem(URN id, IEnumerable<CultureInfo> cultures)
        {
            foreach(var culture in cultures)
            {
                TournamentInfoModel tournamentData;
                try
                {
                    tournamentData = _apiClient.GetTournament(id, culture);
                }
                catch(Exception e)
                {
                    _log.LogError($"Error while fetching tournament {culture.TwoLetterISOLanguageName}: {e}");
                    continue;
                }
                try
                {
                    RefreshOrInsertItem(id, culture, tournamentData);
                }
                catch(Exception e)
                {
                    _log.LogError($"Failed to refresh or load tournament {culture.TwoLetterISOLanguageName}: {e}");
                }
            }
        }

        internal void RefreshOrInsertItem(URN id, CultureInfo culture, TournamentInfoModel model)
        {
            // TODO: Implement
        }

        public void ClearCacheItem(URN id)
        {
            _cache.Remove(id.ToString());
        }
    }
}
