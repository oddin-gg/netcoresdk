using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class Match : IMatch
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Match));

    private readonly IEnumerable<CultureInfo> _cultures;
    private readonly ExceptionHandlingStrategy _handlingStrategy;
    private readonly IMatchCache _matchCache;
    private readonly ISportDataBuilder _sportDataBuilder;

    public Match(
        URN id,
        URN localSportId,
        IMatchCache matchCache,
        ISportDataBuilder sportDataBuilder,
        ExceptionHandlingStrategy handlingStrategy,
        IEnumerable<CultureInfo> cultures)
    {
        Id = id;
        _localSportId = localSportId;
        _matchCache = matchCache;
        _sportDataBuilder = sportDataBuilder;
        _handlingStrategy = handlingStrategy;
        _cultures = cultures;
    }

    private URN _localSportId { get; set; }

    public URN Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    public URN RefId
        => FetchMatch(_cultures)?.RefId;

    public URN SportId => FetchSportId();

    public ITournament Tournament => FetchTournament();

    public IMatchStatus Status
        => _sportDataBuilder.BuildMatchStatus(Id, _cultures);

    public IFixture Fixture
        => _sportDataBuilder.BuildFixture(Id, _cultures);

    public SportFormat SportFormat
    {
        get
        {
            var match = FetchMatch(_cultures);
            return match?.SportFormat;
        }
    }

    public IDictionary<string, string> ExtraInfo
    {
        get
        {
            var match = FetchMatch(_cultures);
            return match?.ExtraInfo;
        }
    }

    private ITeamCompetitor GetHomeAwayCompetitor(bool home)
    {
        var match = FetchMatch(_cultures);
        if (match is null) return null;

        if (match.Competitors.Count() < 2)
        {
            var err = $"Match {match.Id} has less than 2 competitors.";
            _log.LogError(err);
            if (_handlingStrategy == ExceptionHandlingStrategy.THROW)
            {
                throw new ArgumentException(err);
            }

            return null;
        }

        if (!match.SportFormat.IsClassic())
        {
            var err = $"Match {match.Id} is not in a classic sport format. It is: {match.SportFormat}";
            _log.LogError(err);
            if (_handlingStrategy == ExceptionHandlingStrategy.THROW)
            {
                throw new ArgumentException(err);
            }

            return null;
        }

        if (match.Competitors.Count() > 2)
        {
            var err = $"Match {match.Id} has more than 2 competitors.";
            _log.LogError(err);
            if (_handlingStrategy == ExceptionHandlingStrategy.THROW)
            {
                throw new ArgumentException(err);
            }

            return null;
        }

        var c = home ? match.Competitors.FirstOrDefault() : match.Competitors.LastOrDefault();
        var competitor = FetchCompetitor(c.Id);

        if (competitor != null)
        {
            return new TeamCompetitor(c.Qualifier, competitor);
        }

        return null;

    }

    public ITeamCompetitor HomeCompetitor => GetHomeAwayCompetitor(true);

    public ITeamCompetitor AwayCompetitor => GetHomeAwayCompetitor(false);

    public IEnumerable<ITeamCompetitor> Competitors
    {
        get
        {
            var match = FetchMatch(_cultures);
            if (match is null) return null;

            var competitors = new List<ITeamCompetitor>();
            foreach (var c in match.Competitors)
            {
                var competitor = FetchCompetitor(c.Id);
                if (competitor != null)
                {
                    competitors.Add(new TeamCompetitor(c.Qualifier, competitor));
                }
            }

            return competitors;
        }
    }

    public Task<string> GetNameAsync(CultureInfo culture)
    {
        var name = FetchMatch(new[] { culture })?.Name?.FirstOrDefault(d => d.Key.Equals(culture)).Value;
        return Task.FromResult(name);
    }

    public Task<DateTime?> GetScheduledTimeAsync()
        => Task.FromResult(FetchMatch(_cultures)?.ScheduledTime);

    public Task<DateTime?> GetScheduledEndTimeAsync()
        => Task.FromResult(FetchMatch(_cultures)?.ScheduledEndTime);

    public Task<URN> GetSportIdAsync()
        => Task.FromResult(SportId);

    public Task<ISport> GetSportAsync()
        => Task.FromResult(FetchSport(_cultures));

    public LiveOddsAvailability? LiveOddsAvailability
        => FetchMatch(_cultures)?.LiveOddsAvailability;

    private LocalizedMatch FetchMatch(IEnumerable<CultureInfo> cultures)
    {
        var item = _matchCache.GetMatch(Id, cultures);

        if (item is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(Id.ToString(), "Unable to fetch match");
        return item;
    }

    private ICompetitor FetchCompetitor(URN id)
    {
        if (id is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException("null", "Unable to fetch competitor");
        if (id is null)
            return null;
        return _sportDataBuilder.BuildCompetitor(id, _cultures);
    }

    private URN FetchSportId()
    {
        var sportId = _localSportId ?? FetchMatch(_cultures)?.SportId;

        if (sportId is null && _handlingStrategy == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(null, "Cannot load sport");
        if (sportId is null)
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
            throw new ItemNotFoundException("null", "Cannot load tournament");
        if (tournamentId is null)
            return null;
        return _sportDataBuilder.BuildTournament(tournamentId, sportId, _cultures);
    }

    private ISport FetchSport(IEnumerable<CultureInfo> cultures)
    {
        var sportId = SportId;
        if (sportId is null)
            return null;

        return _sportDataBuilder.BuildSport(sportId, cultures);
    }
}