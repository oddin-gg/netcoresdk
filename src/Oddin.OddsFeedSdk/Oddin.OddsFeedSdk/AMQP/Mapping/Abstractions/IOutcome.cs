using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions
{
    public interface IOutcome
    {
        string Id { get; }

        Task<string> GetNameAsync(CultureInfo culture);
    }
}
