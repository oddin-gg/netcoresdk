using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
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
        private readonly ICompetitorCache _competitorCache;
        private readonly IFeedConfiguration _configuration;

        public SportDataBuilder(ISportDataCache sportDataCache, ITournamentsCache tournamentsCache, ICompetitorCache competitorCache, IFeedConfiguration configuration)
        {
            _sportDataCache = sportDataCache;
            _tournamentsCache = tournamentsCache;
            _competitorCache = competitorCache;
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

        public ISport BuildSport(URN id, IEnumerable<CultureInfo> locales)
        {
            return new Sport(
                id,
                locales,
                _sportDataCache,
                this,
                _configuration.ExceptionHandlingStrategy
            );
        }

        public IEnumerable<ITournament> BuildTournaments(IEnumerable<URN> ids, URN sportId, IEnumerable<CultureInfo> locales)
        {
            return ids.Select(t =>             
                new Tournament(
                    t,
                    sportId,
                    _tournamentsCache,
                    this,
                    _configuration,
                    locales)
            );
        }

        public ITournament BuildTournament(URN id, URN sportId, IEnumerable<CultureInfo> locales)
        {
            return new Tournament(
                id, 
                sportId, 
                _tournamentsCache,
                this,
                _configuration, 
                locales);
        }

        public IEnumerable<ICompetitor> BuildCompetitors(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures)
        {
            return ids.Select(id => 
                new Competitor(
                    id,
                    _competitorCache,
                    _configuration.ExceptionHandlingStrategy,
                    cultures));
        }
    }
}
