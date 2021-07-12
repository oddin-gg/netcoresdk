using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    public class BetStopEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly bet_stop _feedMessage;
        private readonly byte[] _rawMessage;
        private readonly IBetStop<T> _betStop;

        internal BetStopEventArgs(IFeedMessageMapper messageMapper, bet_stop feedMessage, byte[] rawMessage)
        {
            if (messageMapper is null)
                throw new ArgumentNullException(nameof(messageMapper));

            if (feedMessage is null)
                throw new ArgumentNullException(nameof(feedMessage));


            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _rawMessage = rawMessage;

            _betStop = GetBetStop();
        }

        /// <summary>
        /// Gets the <see cref="IBetStop{T}"/> implementation representing the received bet stop message translated to the specified languages
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of which to translate the message or a null reference to translate the message
        /// to languages specified in the configuration</param>
        /// <returns>Returns the <see cref="IBetStop{T}"/> implementation representing the received bet stop message translated to the specified languages</returns>
        public IBetStop<T> GetBetStop()
        {
            if ((_betStop is null) == false)
                return _betStop;

            return _messageMapper.MapBetStop<T>(_feedMessage, _rawMessage);
        }
    }
}
