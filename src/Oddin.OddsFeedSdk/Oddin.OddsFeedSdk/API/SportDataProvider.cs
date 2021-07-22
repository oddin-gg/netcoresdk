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
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal interface ISportDataCache 
    {
        IEnumerable<URN> GetSports(IEnumerable<CultureInfo> cultures);

        LocalizedSport GetSport(URN id, IEnumerable<CultureInfo> cultures);
    }

    internal class SportDataCache : ISportDataCache
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(SportDataCache));

        private readonly IApiClient _apiClient;
        private readonly IFeedConfiguration _feedConfiguration;
        private readonly ISportDataBuilder _sportDataBuilder;
        private readonly MemoryCache _cache;
        private readonly

        internal SportDataCache(IApiClient apiClient, IFeedConfiguration feedConfiguration, ISportDataBuilder sportDataBuilder)
        {
            _apiClient = apiClient;
            _feedConfiguration = feedConfiguration;
            _sportDataBuilder = sportDataBuilder;

            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void LoadAndCacheItem(IEnumerable<CultureInfo> cultures)
        {
            foreach (var culture in cultures)
            {
                IEnumerable<sportExtended> sports;
                try
                {
                     GetSports(culture);
                }
                catch (Exception e)
                {
                    _log.LogError($"Error while fetching sports {culture.TwoLetterISOLanguageName}: {e}");
                    continue;
                }
            }
        }

        private IEnumerable<sportExtended> GetSports(CultureInfo culture)
        {
            if (culture is null)
                culture = _feedConfiguration.DefaultLocale;

            var sports = _apiClient.GetSports(culture);

            return sports.sport;
        }

        public LocalizedSport GetSport(URN id, IEnumerable<CultureInfo> cultures)
        {
            var sports = GetSports(cultures);
            if(sports.Any(u => u == id))
                return new LocalizedSport(id, cultures);
            
            return null;
        }
    }

    internal class LocalizedSport : LocalizedItem
    {
        internal URN Id { get; }

        public LocalizedSport(URN id, IEnumerable<CultureInfo> cultures)
            : base(cultures)
        {
            Id = id;
        }
    }

    internal class LocalizedItem : ILocalizedItem
    {
        public IEnumerable<CultureInfo> LoadedLocales { get; }
        
        public LocalizedItem(IEnumerable<CultureInfo> cultures)
        {
            LoadedLocales = cultures;
        }
    }

    internal interface ILocalizedItem 
    {
        IEnumerable<CultureInfo> LoadedLocales { get; }
    }

    internal interface ISportDataBuilder 
    {
        IEnumerable<ISport> BuildSports(LocalizedSport localizedSport);
    }

    internal class SportDataBuilder : ISportDataBuilder
    {
        private readonly ISportDataCache _sportDataCache;

        public SportDataBuilder(ISportDataCache sportDataCache)
        {
            _sportDataCache = sportDataCache;
        }

        public IEnumerable<ISport> BuildSports(LocalizedSport localizedSport)
            => new Sport(localizedSport.Id, _sportDataCache);
    }

    internal class SportDataProvider : ISportDataProvider
    {
        private readonly IApiClient _apiClient;
        private readonly IFeedConfiguration _feedConfiguration;

        internal SportDataProvider(IApiClient apiClient, IFeedConfiguration feedConfiguration)
        {
            _apiClient = apiClient;
            _feedConfiguration = feedConfiguration;
        }

        public Task<ISport> GetSportAsync(URN id, CultureInfo culture = null)
        {   
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<URN>> GetTournamentsAsync(URN id, CultureInfo culture = null)
        {
            throw new NotImplementedException();
        }
    }
}
