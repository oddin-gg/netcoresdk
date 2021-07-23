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


        public async Task<IEnumerable<ITournament>> BuildTournamets(IEnumerable<URN> ids, URN sportId, IEnumerable<CultureInfo> locales)
        {
            // TOOD: Implement
            return new[] { new Tournament() };
        }

        public async Task<ITournament> BuildTournamet(URN id, URN sportId, IEnumerable<CultureInfo> locales)
        {
            // TOOD: Implement
            return new Tournament();
        }
    }
}
