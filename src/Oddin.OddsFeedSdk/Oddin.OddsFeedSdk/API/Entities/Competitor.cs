using System;
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

        public IReadOnlyDictionary<CultureInfo, string> Names
        {
            get
            {
                var names = FetchCompetitor(_cultures)?.Name;
                if(names is not null)
                    return new ReadOnlyDictionary<CultureInfo, string>(names);

                return null;
            }
        }

        public IReadOnlyDictionary<CultureInfo, string> Countries
        {
            get
            {
                var countries = FetchCompetitor(_cultures)?.Country;
                if (countries is not null)
                    return new ReadOnlyDictionary<CultureInfo, string>(countries);

                return null;
            }
        }

        public IReadOnlyDictionary<CultureInfo, string> Abbreviations
        {
            get
            {
                var abbreviations = FetchCompetitor(_cultures)?.Abbreviation;
                if (abbreviations is not null)
                    return new ReadOnlyDictionary<CultureInfo, string>(abbreviations);

                return null;
            }
        }

        public bool? IsVirtual => FetchCompetitor(_cultures)?.IsVirtual;

        public string CountryCode => FetchCompetitor(_cultures)?.CountryCode;

        public string Underage => FetchCompetitor(_cultures)?.Underage;

        public string IconPath => _competitorCache.GetCompetitorIconPath(Id, _cultures.First());

        public Competitor(
            URN id,
            ICompetitorCache competitorCache,
            ISportDataBuilder sportDataBuilder,
            ExceptionHandlingStrategy exceptionHandling,
            IEnumerable<CultureInfo> cultures)
        {
            Id = id;
            _competitorCache = competitorCache;
            _sportDataBuilder = sportDataBuilder;
            _exceptionHandling = exceptionHandling;
            _cultures = cultures;
        }

        public string GetName(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })
                ?.Name
                ?.FirstOrDefault(d => d.Key.Equals(culture))
                .Value;
        }

        public string GetCountry(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })
                ?.Country
                ?.FirstOrDefault(d => d.Key.Equals(culture))
                .Value;
        }

        public string GetAbbreviation(CultureInfo culture)
        {
            return FetchCompetitor(new[] { culture })
                ?.Abbreviation
                ?.FirstOrDefault(d => d.Key.Equals(culture))
                .Value;
        }

        public Task<ISport> GetSportAsync() => Task.FromResult(GetSports()?.FirstOrDefault());

        public IEnumerable<ISport> GetSports()
        {
            var sportIds = FetchCompetitor(_cultures)?.SportIds;

            if (sportIds is null || sportIds.Any() == false)
                return null;
            else
                return sportIds
                    .Select(s => _sportDataBuilder.BuildSport(s, _cultures));
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
