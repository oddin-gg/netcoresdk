using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedPlayer : ILocalizedItem
{
    public LocalizedPlayer(URN id) => Id = id;

    public URN Id { get; }

    internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

    internal IDictionary<CultureInfo, string> FullName { get; set; } = new Dictionary<CultureInfo, string>();

    internal IDictionary<CultureInfo, string> SportID { get; set; } = new Dictionary<CultureInfo, string>();

    public IEnumerable<CultureInfo> LoadedLocals => GetLoadedLocals();

    private IEnumerable<CultureInfo> GetLoadedLocals()
    {
        var allCultures = new HashSet<CultureInfo>();

        foreach (var name in Name)
            allCultures.Add(name.Key);

        foreach (var fullName in FullName)
            allCultures.Add(fullName.Key);

        foreach (var sportID in SportID)
            allCultures.Add(sportID.Key);

        return allCultures;
    }
}
