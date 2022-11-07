using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class Sport : ISport
{
    private readonly ISportDataBuilder _builder;
    private readonly ISportDataCache _cache;
    private readonly IEnumerable<CultureInfo> _cultures;
    private readonly ExceptionHandlingStrategy _exceptionHandling;

    internal Sport(URN id, IEnumerable<CultureInfo> cultures, ISportDataCache cache, ISportDataBuilder builder,
        ExceptionHandlingStrategy exceptionHandling)
    {
        Id = id;
        _cache = cache;
        _builder = builder;
        _exceptionHandling = exceptionHandling;
        _cultures = cultures;
    }

    public URN Id { get; }

    public URN RefId
        => FetchSport(_cultures)?.RefId;

    public string IconPath
        => FetchSport(_cultures)?.IconPath;

    public IReadOnlyDictionary<CultureInfo, string> Names
    {
        get
        {
            var names = FetchSport(_cultures)?.Name;
            if (names is not null)
                return new ReadOnlyDictionary<CultureInfo, string>(names);

            return new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>());
        }
    }

    public IEnumerable<ITournament> Tournaments
        => FetchTournaments();

    public string GetName(CultureInfo culture)
    {
        var sport = FetchSport(new[] { culture });
        return sport.Name?.FirstOrDefault(d => d.Key.Equals(culture)).Value;
    }

    private LocalizedSport FetchSport(IEnumerable<CultureInfo> cultures)
    {
        var sport = _cache.GetSport(Id, cultures).ConfigureAwait(false).GetAwaiter().GetResult();
        if (sport is null && _exceptionHandling == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(Id.ToString(), $"Sport with id {Id} not found");

        return sport;
    }

    private IEnumerable<ITournament> FetchTournaments()
    {
        var sport = _cache.GetSport(Id, _cultures).ConfigureAwait(false).GetAwaiter().GetResult();
        var tournamentIds = sport.TournamentIds ?? _cache.GetSportTournaments(Id, _cultures.First());

        if (tournamentIds == null && _exceptionHandling == ExceptionHandlingStrategy.THROW)
            throw new ItemNotFoundException(Id.ToString(), $"Tournaments not found for sport id {Id}");
        if (tournamentIds == null)
            return null;
        return _builder.BuildTournaments(tournamentIds, Id, _cultures);
    }
}