using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface IPlayerCache : IDisposable
{
    LocalizedPlayer GetPlayer(URN id, IEnumerable<CultureInfo> cultures);

    void ClearCacheItem(URN id);
}
