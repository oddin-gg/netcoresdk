using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class LocalizedOutcomeDescription
{
    [Obsolete("Do not use this field, it will be removed in future.")]
    public int RefId { get; set; }

    internal IDictionary<CultureInfo, string> Name { get; set; } = new Dictionary<CultureInfo, string>();

    internal IDictionary<CultureInfo, string> Description { get; set; } = new Dictionary<CultureInfo, string>();
}