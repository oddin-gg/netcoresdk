using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IOutcomeDescription
{
    long Id { get; }

    long? RefId { get; }

    string GetName(CultureInfo culture);

    string GetDescription(CultureInfo culture);
}