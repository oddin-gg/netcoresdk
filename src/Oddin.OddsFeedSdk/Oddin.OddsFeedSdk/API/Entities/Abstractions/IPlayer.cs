using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface IPlayer
    {
        URN Id { get; }

        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        string GetName(CultureInfo culture);
    }
}
