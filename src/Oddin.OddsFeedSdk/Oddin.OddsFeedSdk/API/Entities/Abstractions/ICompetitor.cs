using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.Common;

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

        Task<ISport> GetSportAsync() => Task.FromResult<ISport>(null);

        string ShortName => null;
    }

    public interface IPlayer
    {
        URN Id { get; }

        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        string GetName(CultureInfo culture);
    }
}
