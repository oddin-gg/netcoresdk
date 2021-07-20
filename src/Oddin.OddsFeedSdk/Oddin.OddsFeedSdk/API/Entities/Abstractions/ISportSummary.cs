using Oddin.OddsFeedSdk.Common;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface ISportSummary
    {
        URN Id { get; }

        string GetName(CultureInfo culture);

        IReadOnlyDictionary<CultureInfo, string> Names { get; }
    }

    public interface ISport : ISportSummary
    {
        IEnumerable<ISportEvent> Tournaments { get; }
    }
}
