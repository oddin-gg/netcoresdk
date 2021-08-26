using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IMarket
    {
        int Id { get; }

        int RefId { get; }

        IReadOnlyDictionary<string, string> Specifiers { get; }

        string ExtendedSpecifiers { get; }

        IEnumerable<string> Groups { get; }

        Task<string> GetNameAsync(CultureInfo culture);
    }
}
