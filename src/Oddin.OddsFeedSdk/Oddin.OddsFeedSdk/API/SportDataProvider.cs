using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal interface ISportDataCache 
    {
        Task<IEnumerable<URN>> GetSports(IEnumerable<CultureInfo> cultures);

        Task<LocalizedSport> GetSport(URN id, IEnumerable<CultureInfo> cultures);
    }

    internal class SportDataCache : ISportDataCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

        private readonly IApiClient _apiClient;
        private readonly MemoryCache _cache;
        private readonly IList<CultureInfo> _loadedLocales = new List<CultureInfo>();

        private readonly SemaphoreSlim _loadAndCacheItemSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _getSemaphore = new SemaphoreSlim(1, 1);

        public SportDataCache(IApiClient apiClient)
        {
            _apiClient = apiClient;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<IEnumerable<URN>> GetSports(IEnumerable<CultureInfo> cultures)
        {
            await _getSemaphore.WaitAsync();
            try
            {
                var culturesToLoad = cultures.Except(_loadedLocales);
                if(culturesToLoad.Any())
                    await LoadAndCacheItem(culturesToLoad);

                return _cache.GetKeys<URN>().Select(key =>
                {
                    var sport = _cache.Get<LocalizedSport>(key);
                    return sport.Id;
                });
            }
            finally
            {
                _getSemaphore.Release();
            }
        }

        public async Task<LocalizedSport> GetSport(URN id, IEnumerable<CultureInfo> cultures)
        {
            await _getSemaphore.WaitAsync();
            try
            {
                throw new NotImplementedException();
                // TODO: implement
            }
            finally
            {
                _getSemaphore.Release();
            }
        }

        private async Task LoadAndCacheItem(IEnumerable<CultureInfo> cultures)
        {
            _loadAndCacheItemSemaphore.Wait();
            try
            {
                foreach (var culture in cultures)
                {
                    SportsModel sports;
                    try
                    {
                        sports = await _apiClient.GetSports(culture);
                    }
                    catch (Exception e)
                    {
                        _log.LogError($"Error while fetching sports {culture.TwoLetterISOLanguageName}: {e}");
                        continue;
                    }

                    foreach(var sport in sports.sport)
                    {
                        var id = new URN(sport.id);
                        try
                        {
                            RefreshOrInsertItem(id, culture, sport);
                        }
                        catch (Exception e)
                        {
                            _log.LogError($"Failed to insert or refresh sport: {e}");
                        }
                    }
                    _loadedLocales.Add(culture);
                }
            }
            finally
            {
                _loadAndCacheItemSemaphore.Release();
            }
        }

        private void RefreshOrInsertItem(URN id, CultureInfo culture, sportExtended sport = null, URN tournamentId = null)
        {
            var isInCache = _cache.TryGetValue<LocalizedSport>(id, out var localizedSport);

            if (isInCache == false)
                localizedSport = new LocalizedSport(id);

            if (sport != null)
                localizedSport.Name[culture] = sport.name;
        
            if(tournamentId != null)
                localizedSport.TournamentIds ??= new List<URN>();
           
            _cache.Set(id, localizedSport, TimeSpan.FromDays(1));
        }
    }

    internal interface ISportDataBuilder 
    {
        Task<IEnumerable<ISport>> BuildSports(IEnumerable<CultureInfo> locales);
    }

    internal class SportDataBuilder : ISportDataBuilder
    {
        private readonly ISportDataCache _sportDataCache;
        private readonly IFeedConfiguration _configuration;

        public SportDataBuilder(ISportDataCache sportDataCache, IFeedConfiguration configuration)
        {
            _sportDataCache = sportDataCache;
            _configuration = configuration;
        }

        public async Task<IEnumerable<ISport>> BuildSports(IEnumerable<CultureInfo> locales)
        {
            var localizedSports = await _sportDataCache.GetSports(locales);
            return localizedSports.Select(s => 
                new Sport(
                    s,
                    locales,
                    _sportDataCache,
                    _configuration.ExceptionHandlingStrategy));
        }
    }

    internal class SportDataProvider : ISportDataProvider
    {
        private readonly IFeedConfiguration _feedConfiguration;
        private readonly ISportDataBuilder _builder;

        public SportDataProvider(IFeedConfiguration feedConfiguration, ISportDataBuilder builder)
        {
            _feedConfiguration = feedConfiguration;
            _builder = builder;
        }

        public Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
        {
            if (culture is null)
                culture = _feedConfiguration.DefaultLocale;

            return _builder.BuildSports(new[] { culture });
        }

        public Task<ISport> GetSportAsync(URN id, CultureInfo culture = null)
        {   
            throw new NotImplementedException();
        }

        public Task<IEnumerable<URN>> GetTournamentsAsync(URN id, CultureInfo culture = null)
        {
            throw new NotImplementedException();
        }
    }
}
