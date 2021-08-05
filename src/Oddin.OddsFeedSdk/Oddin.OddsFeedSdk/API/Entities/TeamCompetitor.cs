using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class TeamCompetitor : ITeamCompetitor
    {
        private readonly string _qualifier;
        private readonly ICompetitor _competitor;

        public TeamCompetitor(string qualifier, ICompetitor competitor)
        {
            _qualifier = qualifier;
            _competitor = competitor;
        }

        public string Qualifier => _qualifier;

        public IReadOnlyDictionary<CultureInfo, string> Countries => _competitor.Countries;

        public IReadOnlyDictionary<CultureInfo, string> Abbreviations => _competitor.Abbreviations;

        public bool? IsVirtual => _competitor.IsVirtual;

        public string CountryCode => _competitor.CountryCode;

        public URN Id => _competitor.Id;

        public URN RefId => _competitor.RefId;

        public IReadOnlyDictionary<CultureInfo, string> Names => _competitor.Names;

        public string GetAbbreviation(CultureInfo culture) => _competitor.GetAbbreviation(culture);

        public string GetCountry(CultureInfo culture) => _competitor.GetCountry(culture);

        public string GetName(CultureInfo culture) => _competitor.GetName(culture);

        public Task<ISport> GetSportAsync() => _competitor.GetSportAsync();
    }
}
