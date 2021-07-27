using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    internal interface ILocalizedItem
    {
        IEnumerable<CultureInfo> LoadedLocals { get; }
    }
}
