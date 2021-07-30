using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface ILocalizedStaticDataCache
    {
        ILocalizedNamedValue Get(long id);

        ILocalizedNamedValue Get(long id, IEnumerable<CultureInfo> cultures);

        bool Exists(long id);
    }
}
