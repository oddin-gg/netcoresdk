using System;
using System.Linq;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class CompositeKey
{
    public readonly int MarketId;
    public readonly string Variant;

    public CompositeKey(int marketId, string variant)
    {
        MarketId = marketId;
        Variant = variant;
    }

    internal string Key => $"{MarketId}-{Variant ?? "*"}";

    public override bool Equals(object obj) => obj is CompositeKey key && Key == key.Key;

    public override int GetHashCode() => HashCode.Combine(Key);

    public override string ToString() => Key;

    public static bool TryParse(string key, out CompositeKey compositeKey)
    {
        compositeKey = null;

        var parts = key.Split('-');
        if (parts.Count() != 2)
            return false;

        if (int.TryParse(parts[0], out var marketId) == false)
            return false;

        compositeKey = new CompositeKey(marketId, parts[1]);
        return true;
    }
}