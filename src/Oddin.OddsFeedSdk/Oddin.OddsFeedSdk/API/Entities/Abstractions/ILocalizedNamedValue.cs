using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface ILocalizedNamedValue : INamedValue
{
    IReadOnlyDictionary<CultureInfo, string> Descriptions { get; }

    string GetDescription(CultureInfo culture);
}