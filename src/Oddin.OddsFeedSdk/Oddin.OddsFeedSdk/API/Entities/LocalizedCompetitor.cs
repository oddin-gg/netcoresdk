using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedCompetitor : ILocalizedItem
{
    public LocalizedCompetitor(URN id) => Id = id;

    public URN Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    public URN RefId { get; set; }

    public IEnumerable<URN> SportIds { get; set; }

    internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

    internal IDictionary<CultureInfo, string> Abbreviation { get; set; } = new Dictionary<CultureInfo, string>();

    internal IDictionary<CultureInfo, IEnumerable<PlayerWithSport>> Players { get; set; } = new Dictionary<CultureInfo, IEnumerable<PlayerWithSport>>();

    internal IDictionary<CultureInfo, string> Country { get; set; } = new Dictionary<CultureInfo, string>();

    public bool? IsVirtual { get; set; }

    public string CountryCode { get; set; }

    public string Underage { get; set; }

    public bool IconPathLoaded { get; set; } = false;

    public string IconPath { get; set; }

    public IEnumerable<CultureInfo> LoadedLocals => GetLoadedLocals();

    private IEnumerable<CultureInfo> GetLoadedLocals()
    {
        var allCultures = new HashSet<CultureInfo>();

        foreach (var name in Name)
            allCultures.Add(name.Key);

        foreach (var abbreviation in Abbreviation)
            allCultures.Add(abbreviation.Key);

        foreach (var players in Players)
            allCultures.Add(players.Key);

        foreach (var country in Country)
            allCultures.Add(country.Key);

        return allCultures;
    }
}
