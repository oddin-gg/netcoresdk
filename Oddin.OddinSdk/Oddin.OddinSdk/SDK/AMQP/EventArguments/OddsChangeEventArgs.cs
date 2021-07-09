using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    public class OddsChangeEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly odds_change _feedMessage;
        private readonly byte[] _rawMessage;
        private readonly IOddsChange<T> _oddsChange;

        internal OddsChangeEventArgs(IFeedMessageMapper messageMapper, odds_change feedMessage, byte[] rawMessage)
        {
            if (messageMapper is null)
                throw new ArgumentNullException($"{nameof(messageMapper)}");

            if (feedMessage is null)
                throw new ArgumentNullException($"{nameof(feedMessage)}");

            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _rawMessage = rawMessage;

            _oddsChange = GetOddsChange();
        }

        /// <summary>
        /// Gets the <see cref="IOddsChange{T}"/> implementation representing the received odds change message translated to the specified languages
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of which to translate the message or a null reference to translate the message
        /// to languages specified in the configuration</param>
        /// <returns>Returns the <see cref="IOddsChange{T}"/> implementation representing the received odds change message translated to the specified languages</returns>
        public IOddsChange<T> GetOddsChange()
        {
            if ((_oddsChange is null) == false)
                return _oddsChange;

            return _messageMapper.MapOddsChange<T>(_feedMessage, _rawMessage);
        }
    }
}
