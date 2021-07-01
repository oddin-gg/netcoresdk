using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;
using System.Globalization;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    public class OddsChangeEventArgs<T> : EventArgs where T : ISportEvent
    {
        /// <summary>
        /// Gets the <see cref="IOddsChange{T}"/> implementation representing the received odds change message translated to the specified languages
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of which to translate the message or a null reference to translate the message
        /// to languages specified in the configuration</param>
        /// <returns>Returns the <see cref="IOddsChange{T}"/> implementation representing the received odds change message translated to the specified languages</returns>
        public IOddsChange<T> GetOddsChange(CultureInfo culture = null)
        {
            // TODO: implement

            throw new NotImplementedException();
        }
    }
}
