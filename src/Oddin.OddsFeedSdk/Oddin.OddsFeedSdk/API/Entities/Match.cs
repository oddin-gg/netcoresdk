using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class Match : IMatch
    {
        private readonly IMatchCache _matchCache;
        private readonly ISportDataBuilder _sportDataBuilder;
        private readonly ExceptionHandlingStrategy _handlingStrategy;
        private readonly IEnumerable<CultureInfo> _cultures;

        public URN Id { get; }

        public URN SportId => FetchSport();

        private URN _localSportId { get; set; }

        public ITournament Tournament => FetchTournament();

        public IMatchStatus Status
            => _sportDataBuilder.BuildMatchStatus(Id, _cultures);

        public IFixture Fixture
            => _sportDataBuilder.BuildFixture(Id, _cultures);

        public ITeamCompetitor HomeCompetitor
        {
            get
            {
                var match = FetchMatch(_cultures);
                if (match is null) return null;

                var competitor = FetchCompetitor(match.HomeTeamId);

                if (competitor != null)
                    return new TeamCompetitor(match.HomeTeamQualifier, competitor);
                else
                    return null;
            }
        }

        public ITeamCompetitor AwayCompetitor
        {
            get
            {
                var match = FetchMatch(_cultures);
                if (match is null) return null;

                var competitor = FetchCompetitor(match.AwayTeamId);

                if (competitor != null)
                    return new TeamCompetitor(match.AwayTeamQualifier, competitor);
                else
                    return null;
            }
        }

        public IEnumerable<ITeamCompetitor> Competitors
        {
            get
            {
                var list = new List<ITeamCompetitor>();
                var homeCompetitor = HomeCompetitor;
                var awayCompetitor = AwayCompetitor;

                if (homeCompetitor != null)
                    list.Add(homeCompetitor);

                if (awayCompetitor != null)
                    list.Add(awayCompetitor);

                return list;
            }
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            var name = FetchMatch(new[] { culture })?.Name?[culture];
            return Task.FromResult(name);
        }

        public Task<DateTime?> GetScheduledTimeAsync()
            => Task.FromResult(FetchMatch(_cultures)?.ScheduledTime);

        public Task<DateTime?> GetScheduledEndTimeAsync()
            => Task.FromResult(FetchMatch(_cultures)?.ScheduledEndTime);

        public Task<URN> GetSportIdAsync()
            => Task.FromResult(SportId);

        public LiveOddsAvailability? LiveOddsAvailability
            => FetchMatch(_cultures)?.LiveOddsAvailability;

        public Match(URN id, URN localSportId, IMatchCache matchCache, ISportDataBuilder sportDataBuilder, ExceptionHandlingStrategy handlingStrategy, IEnumerable<CultureInfo> cultures)
        {
            Id = id;
            _localSportId = localSportId;
            _matchCache = matchCache;
            _sportDataBuilder = sportDataBuilder;
            _handlingStrategy = handlingStrategy;
            _cultures = cultures;
        }

        // TODO: Subscribe and dispose to data  from feed and api

        private LocalizedMatch FetchMatch(IEnumerable<CultureInfo> cultures)
        {
            var item = _matchCache.GetMatch(Id, cultures);

            if (item is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(Id.ToString(), "Unable to fetch competitor");
            else
                return item;
        }

        private ICompetitor FetchCompetitor(URN id)
        {
            if (id is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(id.ToString(), "Unable to fetch competitor");
            else if (id is null)
                return null;
            else
                return _sportDataBuilder.BuildCompetitor(id, _cultures);
        }

        private URN FetchSport()
        {
            var sportId = _localSportId ?? FetchMatch(_cultures)?.SportId;

            if (sportId is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(null, "Cannot load sport");
            else if (sportId is null)
                return null;

            _localSportId = sportId;
            return sportId;
        }

        private ITournament FetchTournament()
        {
            var sportId = SportId;
            if (sportId is null)
                return null;

            var tournamentId = FetchMatch(_cultures)?.TournamentId;

            if (tournamentId is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(tournamentId.ToString(), "Cannot load tournament");
            else if (tournamentId is null)
                return null;
            else
                return _sportDataBuilder.BuildTournament(tournamentId, sportId, _cultures);
        }
    }
}
