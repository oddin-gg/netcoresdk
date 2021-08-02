using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IMatchCache
    {
        void ClearCacheItem(URN id);
        LocalizedMatch GetMatch(URN id, IEnumerable<CultureInfo> cultures);
    }
}