using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    public class OddsChangeEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly odds_change _feedMessage;
        private readonly IReadOnlyCollection<CultureInfo> _defaultCultures;
        private readonly byte[] _rawMessage;
        private readonly IOddsChange<T> _oddsChange;

        internal OddsChangeEventArgs(IFeedMessageMapper messageMapper, odds_change feedMessage, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
        {
            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _defaultCultures = cultures as IReadOnlyCollection<CultureInfo>;
            _rawMessage = rawMessage;

            _oddsChange = GetOddsChange();
        }

        /// <summary>
        /// Gets the <see cref="IOddsChange{T}"/> implementation representing the received odds change message translated to the specified languages
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of which to translate the message or a null reference to translate the message
        /// to languages specified in the configuration</param>
        /// <returns>Returns the <see cref="IOddsChange{T}"/> implementation representing the received odds change message translated to the specified languages</returns>
        public IOddsChange<T> GetOddsChange(CultureInfo culture = null)
        {
            if ((_oddsChange is null) == false
                && culture is null)
                return _oddsChange;

            var defaultCultures = culture is null ? _defaultCultures : new[] { culture };
            return _messageMapper.MapOddsChange<T>(_feedMessage, defaultCultures, _rawMessage);
        }
    }
}
