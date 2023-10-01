using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface ISportSummary
{
    URN Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    URN RefId { get; }

    IReadOnlyDictionary<CultureInfo, string> Names { get; }

    string GetName(CultureInfo culture);
}