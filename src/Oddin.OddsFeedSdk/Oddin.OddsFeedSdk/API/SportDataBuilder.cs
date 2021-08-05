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
        private readonly IMatchCache _matchCache;
        private readonly IMatchStatusCache _matchStatusCache;
        private readonly ILocalizedStaticDataCache _localizedStaticDataCache;
        private readonly IFixtureCache _fixtureCache;
        private readonly IFeedConfiguration _configuration;

        public SportDataBuilder(
            ISportDataCache sportDataCache,
            ITournamentsCache tournamentsCache,
            ICompetitorCache competitorCache,
            IMatchCache matchCache,
            IMatchStatusCache matchStatusCache,
            ILocalizedStaticDataCache localizedStaticDataCache,
            IFixtureCache fixtureCache,
            IFeedConfiguration configuration)
        {
            _sportDataCache = sportDataCache;
            _tournamentsCache = tournamentsCache;
            _competitorCache = competitorCache;
            _matchCache = matchCache;
            _matchStatusCache = matchStatusCache;
            _localizedStaticDataCache = localizedStaticDataCache;
            _fixtureCache = fixtureCache;
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
                    this,
                    _configuration.ExceptionHandlingStrategy,
                    cultures));
        }

        public ICompetitor BuildCompetitor(URN id, IEnumerable<CultureInfo> cultures)
        {
            return new Competitor(
                id,
                _competitorCache,
                this,
                _configuration.ExceptionHandlingStrategy,
                cultures);
        }

        public IEnumerable<IMatch> BuildMatches(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures)
        {
            return ids.Select(id =>
                new Match(
                    id,
                    null,
                    _matchCache,
                    this,
                    _configuration.ExceptionHandlingStrategy,
                    cultures));
        }

        public IMatch BuildMatch(URN id, IEnumerable<CultureInfo> cultures, URN sportId = null)
        {
            return new Match(
                id,
                sportId,
                _matchCache,
                this,
                _configuration.ExceptionHandlingStrategy,
                cultures);
        }

        public IMatchStatus BuildMatchStatus(URN id, IEnumerable<CultureInfo> cultures)
        {
            return new MatchStatus(
                id,
                _matchStatusCache,
                _localizedStaticDataCache,
                _configuration.ExceptionHandlingStrategy,
                cultures);
        }

        public IFixture BuildFixture(URN id, IEnumerable<CultureInfo> cultures)
        {
            return new Fixture(
                id,
                _fixtureCache,
                _configuration.ExceptionHandlingStrategy,
                cultures);
        }
    }
}
