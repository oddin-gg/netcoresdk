using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities
{
    internal class LocalizedSport : ILocalizedItem
    {
        internal URN Id { get; }

        internal URN RefId { get; set; }

        internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

        internal IEnumerable<URN> TournamentIds { get; set; } = null;

        public IEnumerable<CultureInfo> LoadedLocals => Name.Keys;

        public LocalizedSport(URN id)
        {
            Id = id;
        }
    }
}
