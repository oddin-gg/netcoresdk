using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IMarket
{
    int Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    int RefId { get; }

    IReadOnlyDictionary<string, string> Specifiers { get; }

    string ExtendedSpecifiers { get; }

    IEnumerable<string> Groups { get; }

    string GetName(CultureInfo culture);
}