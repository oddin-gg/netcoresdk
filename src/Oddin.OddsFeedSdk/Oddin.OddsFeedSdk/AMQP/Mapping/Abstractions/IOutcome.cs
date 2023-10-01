using System.Globalization;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;

public interface IOutcome
{
    string Id { get; }

    long RefId { get; }

    string GetName(CultureInfo culture);
}