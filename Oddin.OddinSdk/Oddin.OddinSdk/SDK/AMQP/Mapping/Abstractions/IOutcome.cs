using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    /// <summary>
    /// Represent a betting market outcome
    /// </summary>
    public interface IOutcome
    {

        /// <summary>
        /// Gets the value uniquely identifying the current instance
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Asynchronously gets the name of the outcome in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language in which to get the name</param>
        /// <returns>A <see cref="Task{String}"/> representing the async operation</returns>
        Task<string> GetNameAsync(CultureInfo culture);
    }
}
