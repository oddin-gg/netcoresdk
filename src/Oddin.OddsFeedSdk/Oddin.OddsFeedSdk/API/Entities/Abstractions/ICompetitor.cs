using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface ICompetitor : IPlayer
    {
        IReadOnlyDictionary<CultureInfo, string> Countries { get; }

        IReadOnlyDictionary<CultureInfo, string> Abbreviations { get; }

        bool? IsVirtual { get; }

        string GetCountry(CultureInfo culture);

        string GetAbbreviation(CultureInfo culture);

        string CountryCode { get; }

        string Underage { get; }

        string ShortName => null;

        string IconPath { get; }

        // TODO: Delete in next iteration of updates
        [Obsolete("GetSportAsync() is deprecated, please use GetSports() instead. Method GetSportAsync() will be removed in the future.")]
        Task<ISport> GetSportAsync();

        IEnumerable<ISport> GetSports();
    }

    public interface ITeamCompetitor : ICompetitor
    {
        string Qualifier { get; }
    }
}
