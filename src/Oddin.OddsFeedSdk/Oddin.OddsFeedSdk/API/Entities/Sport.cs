using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class Sport : ISport
    {
        private readonly ISportDataCache _cache;
        private readonly ISportDataBuilder _builder;
        private readonly ExceptionHandlingStrategy _exceptionHandling;
        private readonly IEnumerable<CultureInfo> _cultures;

        public URN Id { get; }

        public IReadOnlyDictionary<CultureInfo, string> Names 
            => new ReadOnlyDictionary<CultureInfo, string>(FetchSport()?.Name);

        public IEnumerable<ITournament> Tournaments 
            => FetchTournaments();

        internal Sport(URN id, IEnumerable<CultureInfo> cultures, ISportDataCache cache, ISportDataBuilder builder, ExceptionHandlingStrategy exceptionHandling)
        {
            Id = id;
            _cache = cache;
            _builder = builder;
            _exceptionHandling = exceptionHandling;
            _cultures = cultures;
        }

        public string GetName(CultureInfo culture)
        {
            var sport = FetchSport();
            return sport.Name?.FirstOrDefault(d => d.Key == culture).Value;
        }

        private LocalizedSport FetchSport()
        {
            var sport = _cache.GetSport(Id, _cultures).ConfigureAwait(false).GetAwaiter().GetResult();
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
            else if (tournamentIds == null)
                return null;
            else
                return _builder.BuildTournaments(tournamentIds, Id, _cultures);
        }
    }
}
