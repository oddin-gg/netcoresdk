using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    public class BetStopEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly bet_stop _feedMessage;
        private readonly IReadOnlyList<CultureInfo> _defaultCultures;
        private readonly byte[] _rawMessage;
        private readonly IBetStop<T> _betStop;

        internal BetStopEventArgs(IFeedMessageMapper messageMapper, bet_stop feedMessage, IEnumerable<CultureInfo> cultures, byte[] rawMessage)
        {
            if (messageMapper is null)
                throw new ArgumentNullException($"{nameof(messageMapper)}");

            if (feedMessage is null)
                throw new ArgumentNullException($"{nameof(feedMessage)}");

            if (cultures is null)
                throw new ArgumentNullException($"{nameof(cultures)}");

            if (cultures.Count() <= 0)
                throw new ArgumentException($"{nameof(cultures)} argument cannot be empty!");

            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _defaultCultures = cultures as IReadOnlyList<CultureInfo>;
            _rawMessage = rawMessage;

            _betStop = GetBetStop();
        }

        /// <summary>
        /// Gets the <see cref="IBetStop{T}"/> implementation representing the received bet stop message translated to the specified languages
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of which to translate the message or a null reference to translate the message
        /// to languages specified in the configuration</param>
        /// <returns>Returns the <see cref="IBetStop{T}"/> implementation representing the received bet stop message translated to the specified languages</returns>
        public IBetStop<T> GetBetStop(CultureInfo culture = null)
        {
            if ((_betStop is null) == false
                && culture is null)
                return _betStop;

            var defaultCultures = culture is null ? _defaultCultures : new[] { culture };
            return _messageMapper.MapBetStop<T>(_feedMessage, defaultCultures, _rawMessage);
        }
    }
}
