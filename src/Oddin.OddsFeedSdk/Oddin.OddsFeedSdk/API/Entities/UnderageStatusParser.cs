using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal static class UnderageStatusParser
{
    internal static UnderageStatus Parse(string value) =>
        value switch
        {
            "0" => UnderageStatus.No,
            "1" => UnderageStatus.Yes,
            _ => UnderageStatus.Unknown
        };
}
