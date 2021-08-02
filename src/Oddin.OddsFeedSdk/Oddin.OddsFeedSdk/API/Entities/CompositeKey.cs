using System;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class CompositeKey
    {
        private readonly int _marketId;
        private readonly string _variant;

        internal string Key => $"{_marketId}-{_variant ?? "*"}";

        public CompositeKey(int marketId, string variant)
        {
            _marketId = marketId;
            _variant = variant;
        }

        public override bool Equals(object obj) => obj is CompositeKey key && Key == key.Key;

        public override int GetHashCode() => HashCode.Combine(Key);
    }
}
