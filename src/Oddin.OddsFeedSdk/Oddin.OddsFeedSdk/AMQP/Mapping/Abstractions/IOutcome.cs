using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IOutcome
    {
        long Id { get; }

        long RefId { get; }

        Task<string> GetNameAsync(CultureInfo culture);
    }
}
