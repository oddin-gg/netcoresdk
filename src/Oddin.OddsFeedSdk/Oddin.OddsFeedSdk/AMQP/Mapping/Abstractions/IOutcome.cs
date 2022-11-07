using System.Globalization;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IOutcome
{
    long Id { get; }

    long RefId { get; }

    string GetName(CultureInfo culture);
}