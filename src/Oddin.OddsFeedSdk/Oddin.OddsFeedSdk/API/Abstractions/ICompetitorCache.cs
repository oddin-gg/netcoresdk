using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface ICompetitorCache : IDisposable
{
    LocalizedCompetitor GetCompetitor(URN id, IEnumerable<CultureInfo> cultures);

    string GetCompetitorIconPath(URN id, CultureInfo culture);

    void ClearCacheItem(URN id);
}