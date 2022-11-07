using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface IMatchCache : IDisposable
{
    void ClearCacheItem(URN id);
    LocalizedMatch GetMatch(URN id, IEnumerable<CultureInfo> cultures);
    void OnFeedMessageReceived(fixture_change e);
}