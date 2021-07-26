using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.API
{
    internal interface ICacheManager
    {
        ISportDataCache SportDataCache { get; }

        ITournamentsCache TournamentsCache { get; }
    }

    internal class CacheManager : ICacheManager
    {
        public ISportDataCache SportDataCache { get; }

        public ITournamentsCache TournamentsCache { get; }

        public CacheManager(ISportDataCache sportDataCache, ITournamentsCache tournamentsCache)
        {
            SportDataCache = sportDataCache;
            TournamentsCache = tournamentsCache;
        }
    }
}
