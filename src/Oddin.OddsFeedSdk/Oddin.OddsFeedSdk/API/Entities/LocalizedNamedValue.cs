using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedNamedValue : ILocalizedNamedValue
{
    private readonly IDictionary<CultureInfo, string> _descriptionMap;

    public LocalizedNamedValue(long id, IDictionary<CultureInfo, string> descriptionMap)
    {
        Id = id;
        _descriptionMap = descriptionMap;
    }

    public long Id { get; }

    public IReadOnlyDictionary<CultureInfo, string> Descriptions
        => _descriptionMap is null
            ? new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>())
            : new ReadOnlyDictionary<CultureInfo, string>(_descriptionMap);

    public string Description => _descriptionMap.Values.FirstOrDefault();

    public string GetDescription(CultureInfo culture) =>
        _descriptionMap?.FirstOrDefault(d => d.Key.Equals(culture)).Value;
}