using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    public class BetCancelEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly bet_cancel _feedMessage;
        private readonly byte[] _rawMessage;
        private readonly IBetCancel<T> _betCancel;

        internal BetCancelEventArgs(IFeedMessageMapper messageMapper, bet_cancel feedMessage, byte[] rawMessage)
        {
            if (messageMapper is null)
                throw new ArgumentNullException($"{nameof(messageMapper)}");

            if (feedMessage is null)
                throw new ArgumentNullException($"{nameof(feedMessage)}");

            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _rawMessage = rawMessage;

            _betCancel = GetBetCancel();
        }

        public IBetCancel<T> GetBetCancel()
        {
            if ((_betCancel is null) == false)
                return _betCancel;

            return _messageMapper.MapBetCancel<T>(_feedMessage, _rawMessage);
        }
    }
}
