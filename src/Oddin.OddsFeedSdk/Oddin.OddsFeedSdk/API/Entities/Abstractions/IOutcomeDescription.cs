using System;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IOutcomeDescription
{
    string Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    long? RefId { get; }

    string GetName(CultureInfo culture);

    string GetDescription(CultureInfo culture);
}