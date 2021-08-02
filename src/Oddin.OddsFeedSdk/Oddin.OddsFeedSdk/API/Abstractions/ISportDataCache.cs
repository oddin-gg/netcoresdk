using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface ISportDataCache : IDisposable
    {
        Task<IEnumerable<URN>> GetSports(IEnumerable<CultureInfo> cultures);

        Task<LocalizedSport> GetSport(URN id, IEnumerable<CultureInfo> cultures);

        IEnumerable<URN> GetSportTournaments(URN id, CultureInfo culture);
    }
}
