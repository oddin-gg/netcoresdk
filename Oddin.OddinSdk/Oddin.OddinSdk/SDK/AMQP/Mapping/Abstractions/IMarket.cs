using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions
{
    /// <summary>
    /// Represents a betting market
    /// </summary>
    public interface IMarket
    {
        /// <summary>
        /// Gets a <see cref="int"/> value specifying the market type
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers
        /// </summary>
        /// <remarks>Note that the <see cref="Id"/> and <see cref="Specifiers"/> combined uniquely identify the market within the event</remarks>
        IReadOnlyDictionary<string, string> Specifiers { get; }

        /// <summary>
        /// Asynchronously gets the name of the market in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language in which to get the name</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the async operation</returns>
        Task<string> GetNameAsync(CultureInfo culture);
    }
}
