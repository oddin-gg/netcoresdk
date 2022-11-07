using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IMarketDescription
{
    int Id { get; }

    int? RefId { get; }

    IEnumerable<IOutcomeDescription> Outcomes { get; }

    IEnumerable<ISpecifier> Specifiers { get; }

    /// <summary>
    ///     Variant
    /// </summary>
    string OutcomeType { get; }

    string GetName(CultureInfo culture);
}