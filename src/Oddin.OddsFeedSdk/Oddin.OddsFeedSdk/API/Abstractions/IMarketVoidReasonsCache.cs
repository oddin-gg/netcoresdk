using System.Collections.Generic;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface IMarketVoidReasonsCache
{
    IEnumerable<IMarketVoidReason> GetMarketVoidReasons();
}