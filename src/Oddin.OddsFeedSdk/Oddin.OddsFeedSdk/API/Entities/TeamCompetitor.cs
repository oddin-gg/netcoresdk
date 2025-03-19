using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class TeamCompetitor : ITeamCompetitor
{
    private readonly ICompetitor _competitor;

    public TeamCompetitor(string qualifier, ICompetitor competitor)
    {
        Qualifier = qualifier;
        _competitor = competitor;
    }

    public string Qualifier { get; }

    public IReadOnlyDictionary<CultureInfo, string> Countries => _competitor.Countries;

    public IReadOnlyDictionary<CultureInfo, string> Abbreviations => _competitor.Abbreviations;

    public bool? IsVirtual => _competitor.IsVirtual;

    public string CountryCode => _competitor.CountryCode;

    public string Underage => _competitor.Underage;

    public URN Id => _competitor.Id;

    [Obsolete("Do not use this field, it will be removed in future.")]
    public URN RefId => _competitor.RefId;

    public string IconPath => _competitor.IconPath;

    public IReadOnlyDictionary<CultureInfo, string> Names => _competitor.Names;

    public string GetAbbreviation(CultureInfo culture) => _competitor.GetAbbreviation(culture);

    public string GetCountry(CultureInfo culture) => _competitor.GetCountry(culture);

    public string GetName(CultureInfo culture) => _competitor.GetName(culture);

    public Task<ISport> GetSportAsync() => _competitor.GetSportAsync();

    public IEnumerable<ISport> GetSports() => _competitor.GetSports();

    public IEnumerable<IPlayer> GetPlayers() => _competitor.GetPlayers();
}