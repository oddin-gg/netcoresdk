using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
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

        public IBetStop<T> GetBetStop()
        {
            if ((_betStop is null) == false)
                return _betStop;

            return _messageMapper.MapBetStop<T>(_feedMessage, _rawMessage);
        }
    }
}
