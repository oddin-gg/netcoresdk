using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class Tournament : ITournament
{
    private readonly IFeedConfiguration _configuration;
    private readonly IEnumerable<CultureInfo> _cultures;
    private readonly ISportDataBuilder _sportDataBuilder;
    private readonly URN _sportId;
    private readonly ITournamentsCache _tournamentsCache;

    public Tournament(URN id, URN sportId, ITournamentsCache tournamentsCache, ISportDataBuilder sportDataBuilder,
        IFeedConfiguration configuration, IEnumerable<CultureInfo> cultures)
    {
        Id = id;
        _sportId = sportId;
        _tournamentsCache = tournamentsCache;
        _sportDataBuilder = sportDataBuilder;
        _configuration = configuration;
        _cultures = cultures;
    }

    public URN Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    public URN RefId
        => FetchTournament(_cultures)?.RefId;

    public string IconPath => FetchTournament(_cultures)?.IconPath;

    public Task<string> GetNameAsync(CultureInfo culture)
        => Task.FromResult(GetName(culture));

    public Task<DateTime?> GetScheduledTimeAsync()
        => Task.FromResult(FetchTournament(_cultures)?.ScheduledTime);

    public Task<DateTime?> GetScheduledEndTimeAsync()
        => Task.FromResult(FetchTournament(_cultures)?.ScheduledEndTime);

    public Task<URN> GetSportIdAsync()
        => Task.FromResult(FetchSport(_cultures).Id);

    public Task<ISport> GetSportAsync()
        => Task.FromResult(FetchSport(_cultures));

    public IEnumerable<ICompetitor> GetCompetitors()
        => FetchCompetitors(_cultures);

    public DateTime? GetStartDate()
        => FetchTournament(_cultures)?.StartDate;

    public DateTime? GetEndDate()
        => FetchTournament(_cultures)?.EndDate;

    public int? RiskTier()
        => FetchTournament(_cultures)?.RiskTier;

    private string GetName(CultureInfo culture)
        => FetchTournament(new[] { culture })
            ?.Name
            ?.FirstOrDefault(d => d.Key.Equals(culture))
            .Value;

    private LocalizedTournament FetchTournament(IEnumerable<CultureInfo> cultures)
    {
        var tournament = _tournamentsCache.GetTournament(Id, cultures);
        if (tournament is null && _configuration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(Id.ToString(), $"Competitor {Id} not found");
        return tournament;
    }

    private ISport FetchSport(IEnumerable<CultureInfo> cultures) => _sportDataBuilder.BuildSport(_sportId, cultures);

    private IEnumerable<ICompetitor> FetchCompetitors(IEnumerable<CultureInfo> cultures)
    {
        var competitorsIds = FetchTournament(cultures)?.CompetitorIds
                             ?? _tournamentsCache.GetTournamentCompetitors(Id, cultures.First());

        if (competitorsIds == null && _configuration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(Id.ToString(), "Cannot find competitor ids");
        if (competitorsIds == null)
            return null;
        return _sportDataBuilder.BuildCompetitors(competitorsIds, cultures);
    }
}