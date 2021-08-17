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

        int? Underage { get; }

        Task<ISport> GetSportAsync();

        string ShortName => null;
    }

    public interface ITeamCompetitor : ICompetitor
    {
        string Qualifier { get; }
    }
}
