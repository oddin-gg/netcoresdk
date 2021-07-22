using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class Sport : ISport
    {
        private readonly ISportDataCache _cache;
        private readonly ExceptionHandlingStrategy _exceptionHandling;
        private readonly IEnumerable<CultureInfo> _cultures;

        public URN Id { get; }

        public IReadOnlyDictionary<CultureInfo, string> Names { get; }
        
        public IEnumerable<ISportEvent> Tournaments => throw new NotImplementedException();

        internal Sport(URN id, IEnumerable<CultureInfo> cultures, ISportDataCache cache, ExceptionHandlingStrategy exceptionHandling)
        {
            Id = id;
            _cache = cache;
            _exceptionHandling = exceptionHandling;
            _cultures = cultures;
        }

        public string GetName(CultureInfo culture)
        {
            var sport = FetchSport();
        }

        private LocalizedSport FetchSport()
        {
            var sport = _cache.GetSport(Id, _cultures);
            if (sport is null && _exceptionHandling == ExceptionHandlingStrategy.THROW)
                throw new ItemNotFoundException(Id.ToString(), $"Sport with id {Id} not found");

            return sport;
        }
    }
}
