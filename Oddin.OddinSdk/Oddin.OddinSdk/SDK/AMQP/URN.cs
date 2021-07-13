using System;

namespace Oddin.OddinSdk.SDK.AMQP
{
    /// <summary>
    /// Represents a Uniform Resource Name
    /// </summary>
    public class URN
    {
        public string Prefix { get; }
        
        public string Type { get; }

        public long Id { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="URN"/>. The <see cref="URN"/> structure is <code>prefix:type:id</code>
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <param name="type">The type</param>
        /// <param name="id">The id</param>
        public URN(string prefix, string type, long id)
        {
            Prefix = prefix;
            Type = type;
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="URN"/>. The <see cref="URN"/> structure is <code>prefix:type:id</code>
        /// </summary>
        /// <param name="urn">The complete URN string</param>
        public URN(string urn)
        {
            if (urn is null)
                throw new ArgumentNullException(nameof(urn));

            if (TryParseUrn(urn, out var prefix, out var type, out var id) == false)
                throw new ArgumentException($"Given argument {nameof(urn)} of type {typeof(string).Name} is not a valid {typeof(URN).Name}!");

            Prefix = prefix;
            Type = type;
            Id = id;
        }

        public override string ToString()
            => $"{Prefix}:{Type}:{Id}";

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
