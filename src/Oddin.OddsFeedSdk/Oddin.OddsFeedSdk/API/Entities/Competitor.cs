using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class Competitor : ICompetitor
    {
        private readonly ICompetitorCache _competitorCache;
        private readonly ExceptionHandlingStrategy _exceptionHandling;
        private readonly IEnumerable<CultureInfo> _cultures;

        public URN Id { get; }

        public IReadOnlyDictionary<CultureInfo, string> Names => new ReadOnlyDictionary<CultureInfo, string>(FetchCompetitor(_cultures)?.Name);

        public IReadOnlyDictionary<CultureInfo, string> Countries => new ReadOnlyDictionary<CultureInfo, string>(FetchCompetitor(_cultures)?.Country);

        public IReadOnlyDictionary<CultureInfo, string> Abbreviations => new ReadOnlyDictionary<CultureInfo, string>(FetchCompetitor(_cultures)?.Abbreviation);

        public bool? IsVirtual => FetchCompetitor(_cultures)?.IsVirtual;

        public string CountryCode => FetchCompetitor(_cultures)?.CountryCode;

        public Competitor(URN id, ICompetitorCache competitorCache, ExceptionHandlingStrategy exceptionHandling, IEnumerable<CultureInfo> cultures)
        {
            Id = id;
            _competitorCache = competitorCache;
            _exceptionHandling = exceptionHandling;
            _cultures = cultures;
        }

        public string GetName(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })?.Name?[culture];
        }

        private LocalizedCompetitor FetchCompetitor(IEnumerable<CultureInfo> cultures)
        {
            var item = _competitorCache.GetCompetitor(Id, cultures);

            if (item == null && _exceptionHandling == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(Id.ToString(), $"Competitor {Id} not found");
            else
                return item;
        }

        public string GetCountry(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })?.Country?[culture];
        }

        public string GetAbbreviation(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })?.Abbreviation?[culture];
        }
    }

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

        public IReadOnlyDictionary<CultureInfo, string> Names => _competitor.Names;

        public string GetAbbreviation(CultureInfo culture) => _competitor.GetAbbreviation(culture);

        public string GetCountry(CultureInfo culture) => _competitor.GetCountry(culture);

        public string GetName(CultureInfo culture) => _competitor.GetName(culture);
    }
}
