using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface ITournamentsCache : IDisposable
    {
        LocalizedTournament GetTournament(URN id, IEnumerable<CultureInfo> cultures);

        IEnumerable<URN> GetTournamentCompetitors(URN id, CultureInfo culture);

        void ClearCacheItem(URN id);

        void OnFeedMessageReceived(fixture_change e);
    }
}