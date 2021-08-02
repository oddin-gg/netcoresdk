using System;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IMatchStatusCache : IDisposable
    {
        void ClearCacheItem(URN id);

        LocalizedMatchStatus GetMatchStatus(URN id);
    }
}
