using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface ISportDataBuilder
    {
        Task<IEnumerable<ISport>> BuildSports(IEnumerable<CultureInfo> locales);

        IEnumerable<ITournament> BuildTournamets(IEnumerable<URN> ids, URN sportId, IEnumerable<CultureInfo> locales);

        ITournament BuildTournamet(URN id, URN sportId, IEnumerable<CultureInfo> locales);
    }
}
