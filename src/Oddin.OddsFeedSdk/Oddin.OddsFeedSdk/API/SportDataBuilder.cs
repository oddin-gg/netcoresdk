using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal class SportDataBuilder : ISportDataBuilder
    {
        private readonly ISportDataCache _sportDataCache;
        private readonly ITournamentsCache _tournamentsCache;
        private readonly IFeedConfiguration _configuration;

        public SportDataBuilder(ISportDataCache sportDataCache, ITournamentsCache tournamentsCache, IFeedConfiguration configuration)
        {
            _sportDataCache = sportDataCache;
            _tournamentsCache = tournamentsCache;
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
                    this,
                    _configuration.ExceptionHandlingStrategy));
        } 
        
        public async Task<IEnumerable<ISport>> BuildTournaments(List<URN> ids, URN sportId, IEnumerable<CultureInfo> locales)
        {
            var localizedSports = await _sportDataCache.GetSports(locales);
            return localizedSports.Select(s => 
                new Sport(
                    s,
                    locales,
                    _sportDataCache,
                    this,
                    _configuration.ExceptionHandlingStrategy));
        }

        public IEnumerable<ITournament> BuildTournamets(IEnumerable<URN> ids, URN sportId, IEnumerable<CultureInfo> locales)
        {
            return ids.Select(t =>             
                new Tournament(
                    t,
                    sportId,
                    _tournamentsCache,
                    _configuration,
                    locales)
            );
        }

        public ITournament BuildTournamet(URN id, URN sportId, IEnumerable<CultureInfo> locales)
        {
            return new Tournament(
                id, 
                sportId, 
                _tournamentsCache, 
                _configuration, 
                locales);
        }
    }
}
