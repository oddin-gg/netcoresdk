using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Enums;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IMarketDescription
{
    int Id { get; }

    int? RefId { get; }

    IEnumerable<IOutcomeDescription> Outcomes { get; }

    IEnumerable<ISpecifier> Specifiers { get; }

    IEnumerable<string> Groups { get; }

    /// <summary>
    ///     Variant
    /// </summary>
    string Variant { get; }

    string IncludesOutcomesOfType { get; }

    /// <summary>
    ///     OutcomeType
    /// </summary>
    OutcomeType? OutcomeType { get; }

    string GetName(CultureInfo culture);
}