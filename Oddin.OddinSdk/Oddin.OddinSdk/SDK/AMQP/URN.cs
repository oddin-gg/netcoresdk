using System;

namespace Oddin.OddinSdk.SDK.AMQP
{
    public class URN
    {
        public string Prefix { get; }
        
        public string Type { get; }

        public long Id { get; }

        public string Urn => $"{Prefix}:{Type}:{Id}";

        public URN(string prefix, string type, long id)
        {
            Prefix = prefix;
            Type = type;
            Id = id;
        }

        public URN(string urn)
        {
            if (urn is null)
                throw new ArgumentNullException($"{nameof(urn)}");

            if (TryParseUrn(urn, out var prefix, out var type, out var id) == false)
                throw new ArgumentException($"Given argument {nameof(urn)} of type {typeof(string).Name} is not a valid {typeof(URN).Name}");

            Prefix = prefix;
            Type = type;
            Id = id;
        }

        private bool TryParseUrn(string urnString, out string prefix, out string type, out long id)
        {
            prefix = string.Empty;
            type = string.Empty;
            id = 0;

            var urnSplit = urnString.Split(":", StringSplitOptions.RemoveEmptyEntries);
            if (urnSplit.Length != 3)
                return false;

            if (long.TryParse(urnSplit[2], out id) == false
                || id <= 0)
                return false;

            prefix = urnSplit[0];
            type = urnSplit[1];
            return true;
        }
    }
}
