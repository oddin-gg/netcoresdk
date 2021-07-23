using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    internal class SportDataProvider : ISportDataProvider
    {
        private readonly IFeedConfiguration _feedConfiguration;
        private readonly ISportDataBuilder _builder;

        public SportDataProvider(IFeedConfiguration feedConfiguration, ISportDataBuilder builder)
        {
            _feedConfiguration = feedConfiguration;
            _builder = builder;
        }

        public async Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
        {
            if (culture is null)
                culture = _feedConfiguration.DefaultLocale;

            return await _builder.BuildSports(new[] { culture });
        }

        public async Task<ISport> GetSportAsync(URN id, CultureInfo culture = null)
        {
            if (culture is null)
                culture = _feedConfiguration.DefaultLocale;

            var sports = await _builder.BuildSports(new[] { culture });
            return sports.FirstOrDefault(s => s.Id == id);
        }

        public IEnumerable<ITournament> GetActiveTournaments(CultureInfo culture)
        {
            var sports = GetSportsAsync(culture).ConfigureAwait(false).GetAwaiter().GetResult();
            var sportTournaments = sports.SelectMany(s => s.Tournaments);

            return new HashSet<ITournament>(sportTournaments, new TournamentIdComparer());
        }

        public IEnumerable<ITournament> GetActiveTournaments(string name, CultureInfo culture = null)
        {
            var sports = GetSportsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var sport = sports?.FirstOrDefault(s => s.GetName(culture).Equals(name));
            return sport.Tournaments ?? new List<ITournament>();
        }

        internal class TournamentIdComparer : IEqualityComparer<ITournament>
        {
            public bool Equals(ITournament one, ITournament two)
                => one.Id == two.Id;
        
            public int GetHashCode(ITournament item)
                => item.GetHashCode();
        }
    }
}
