using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    public interface ISportDataProvider
    {
        Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null);

        Task<ISport> GetSportAsync(URN id, CultureInfo culture = null);

        IEnumerable<ITournament> GetActiveTournaments(CultureInfo culture);

        IEnumerable<ITournament> GetActiveTournaments(string name, CultureInfo culture = null);
    }
}
