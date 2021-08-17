using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ISportDataBuilder _sportDataBuilder;
        private readonly ExceptionHandlingStrategy _exceptionHandling;
        private readonly IEnumerable<CultureInfo> _cultures;

        public URN Id { get; }

        public URN RefId => FetchCompetitor(_cultures)?.RefId;

        public IReadOnlyDictionary<CultureInfo, string> Names => new ReadOnlyDictionary<CultureInfo, string>(FetchCompetitor(_cultures)?.Name);

        public IReadOnlyDictionary<CultureInfo, string> Countries => new ReadOnlyDictionary<CultureInfo, string>(FetchCompetitor(_cultures)?.Country);

        public IReadOnlyDictionary<CultureInfo, string> Abbreviations => new ReadOnlyDictionary<CultureInfo, string>(FetchCompetitor(_cultures)?.Abbreviation);

        public bool? IsVirtual => FetchCompetitor(_cultures)?.IsVirtual;

        public string CountryCode => FetchCompetitor(_cultures)?.CountryCode;

        public string Underage => FetchCompetitor(_cultures)?.Underage;

        public Competitor(URN id, ICompetitorCache competitorCache, ISportDataBuilder sportDataBuilder, ExceptionHandlingStrategy exceptionHandling, IEnumerable<CultureInfo> cultures)
        {
            Id = id;
            _competitorCache = competitorCache;
            _sportDataBuilder = sportDataBuilder;
            _exceptionHandling = exceptionHandling;
            _cultures = cultures;
        }

        public string GetName(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })?.Name?.FirstOrDefault(d => d.Key == culture).Value;
        }

        public string GetCountry(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })?.Country?.FirstOrDefault(d => d.Key == culture).Value;
        }

        public string GetAbbreviation(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })?.Abbreviation?.FirstOrDefault(d => d.Key == culture).Value;
        }

        public Task<ISport> GetSportAsync()
        {
            var sportId = FetchCompetitor(_cultures)?.SportId;

            if (sportId == null)
                return Task.FromResult<ISport>(null);
            else
                return Task.FromResult(_sportDataBuilder.BuildSport(sportId, _cultures));
        }

        private LocalizedCompetitor FetchCompetitor(IEnumerable<CultureInfo> cultures)
        {
            var item = _competitorCache.GetCompetitor(Id, cultures);

            if (item == null && _exceptionHandling == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(Id.ToString(), $"Competitor {Id} not found");
            else
                return item;
        }
    }
}
