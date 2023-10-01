using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface ICompetitor
{
    URN Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    URN RefId { get; }

    IReadOnlyDictionary<CultureInfo, string> Names { get; }

    IReadOnlyDictionary<CultureInfo, string> Countries { get; }

    IReadOnlyDictionary<CultureInfo, string> Abbreviations { get; }

    bool? IsVirtual { get; }

    string CountryCode { get; }

    string Underage { get; }

    string ShortName => null;

    string IconPath { get; }

    string GetName(CultureInfo culture);

    string GetCountry(CultureInfo culture);

    string GetAbbreviation(CultureInfo culture);

    // TODO: Delete in next iteration of updates
    [Obsolete(
        "GetSportAsync() is deprecated, please use GetSports() instead. Method GetSportAsync() will be removed in the future.")]
    Task<ISport> GetSportAsync();

    IEnumerable<ISport> GetSports();
}

public interface ITeamCompetitor : ICompetitor
{
    string Qualifier { get; }
}