using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IPlayer
{
    URN Id { get; }

    IReadOnlyDictionary<CultureInfo, string> Names { get; }

    IReadOnlyDictionary<CultureInfo, string> FullNames { get; }

    IReadOnlyDictionary<CultureInfo, string> SportIDs { get; }

    string GetName(CultureInfo culture);

    string GetFullName(CultureInfo culture);

    string GetSportID(CultureInfo culture);
}
