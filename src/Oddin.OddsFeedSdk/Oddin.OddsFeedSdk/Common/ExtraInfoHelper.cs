using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Models;

namespace Oddin.OddsFeedSdk.Common;

internal static class ExtraInfoHelper
{
    /// <summary>
    ///     Builds an extra_info dictionary that tolerates malformed payloads instead of throwing:
    ///     null keys are skipped, and a duplicate key uses the last value with a warning. Both
    ///     <c>MatchCache</c> and <c>FixtureCache</c> call this single builder so their tolerance
    ///     rules cannot drift apart. Returns <c>null</c> when the source is <c>null</c>, preserving
    ///     the "absent" signal callers rely on to distinguish absent from empty.
    /// </summary>
    public static IDictionary<string, string> ToExtraInfoDictionary(
        IEnumerable<info> extraInfoItems,
        URN id,
        ILogger log)
    {
        if (extraInfoItems is null)
            return null;

        var result = new Dictionary<string, string>();
        foreach (var item in extraInfoItems)
        {
            if (item?.key is null)
                continue;

            if (result.ContainsKey(item.key))
            {
                log.LogWarning($"Duplicate extra_info key '{item.key}' for event '{id}', using the last value.");
            }

            result[item.key] = item.value;
        }

        return result;
    }
}
