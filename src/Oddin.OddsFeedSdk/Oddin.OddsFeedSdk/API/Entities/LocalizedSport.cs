using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedSport : ILocalizedItem
{
    public LocalizedSport(URN id) => Id = id;

    internal URN Id { get; }

    internal URN RefId { get; set; }

    internal string IconPath { get; set; }

    internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

    internal ICollection<URN> TournamentIds { get; set; } = null;

    public IEnumerable<CultureInfo> LoadedLocals => Name.Keys;
}