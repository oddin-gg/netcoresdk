using Oddin.OddsFeedSdk.AMQP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API
{
    //public interface ISportDataProvider
    //{
    //    Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null);

    //    Task<ISport> GetSportAsync(URN id, CultureInfo culture = null);


    //    /// <summary>
    //    /// Gets a <see cref="ILongTermEvent"/> representing the specified tournament in language specified by <code>culture</code> or a null reference if the tournament with
    //    /// specified <code>id</code> does not exist
    //    /// </summary>
    //    /// <param name="id">A <see cref="URN"/> specifying the tournament to retrieve</param>
    //    /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
    //    /// <returns>A <see cref="ILongTermEvent"/> representing the specified tournament or a null reference if requested tournament does not exist</returns>
    //    ILongTermEvent GetTournament(URN id, CultureInfo culture = null);

    //    /// <summary>
    //    /// Asynchronously gets a list of <see cref="IEnumerable{ICompetition}"/>
    //    /// </summary>
    //    /// <remarks>Lists almost all events we are offering prematch odds for. This endpoint can be used during early startup to obtain almost all fixtures. This endpoint is one of the few that uses pagination.</remarks>
    //    /// <param name="startIndex">Starting record (this is an index, not time)</param>
    //    /// <param name="limit">How many records to return (max: 1000)</param>
    //    /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
    //    /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
    //    Task<IEnumerable<ICompetition>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture = null);

    //    /// <summary>
    //    /// Asynchronously gets a list of active <see cref="IEnumerable{ISportEvent}"/>
    //    /// </summary>
    //    /// <remarks>Lists all <see cref="ISportEvent"/> that are cached (once schedule is loaded)</remarks>
    //    /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
    //    /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
    //    Task<IEnumerable<ISportEvent>> GetActiveTournamentsAsync(CultureInfo culture = null);

    //    /// <summary>
    //    /// Asynchronously gets a list of available <see cref="IEnumerable{ISportEvent}"/> for a specific sport
    //    /// </summary>
    //    /// <remarks>Lists all available tournaments for a sport event we provide coverage for</remarks>
    //    /// <param name="sportId">A <see cref="URN"/> specifying the sport to retrieve</param>
    //    /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
    //    /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
    //    Task<IEnumerable<ISportEvent>> GetAvailableTournamentsAsync(URN sportId, CultureInfo culture = null);
    //}
}
