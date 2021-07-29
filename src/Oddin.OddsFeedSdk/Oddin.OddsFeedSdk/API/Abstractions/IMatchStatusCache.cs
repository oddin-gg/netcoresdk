using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IMatchStatusCache
    {
        void ClearCacheItem(URN id);
        LocalizedMatchStatus GetMatchStatus(URN id);
    }
}
