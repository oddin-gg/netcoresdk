using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Entities
{
    // TOOD: Implement
    internal class Tournament : ITournament
    {
        private readonly URN _sportId;
        private readonly ITournamentsCache _tournamentsCache;
        private readonly IFeedConfiguration _configuration;
        private readonly IEnumerable<CultureInfo> _cultures;

        public URN Id { get; }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime?> GetScheduledEndTimeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DateTime?> GetScheduledTimeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<URN> GetSportIdAsync()
        {
            throw new NotImplementedException();
        }

        public Tournament(URN id, URN sportId, ITournamentsCache tournamentsCache, IFeedConfiguration configuration, IEnumerable<CultureInfo> cultures)
        {
            Id = id;
            _sportId = sportId;
            _tournamentsCache = tournamentsCache;
            _configuration = configuration;
            _cultures = cultures;
        }
    }
}
