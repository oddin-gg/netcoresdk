using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
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
                throw new ArgumentNullException(nameof(messageMapper));

            if (feedMessage is null)
                throw new ArgumentNullException(nameof(feedMessage));

            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _rawMessage = rawMessage;

            _oddsChange = GetOddsChange();
        }

        public IOddsChange<T> GetOddsChange()
        {
            if ((_oddsChange is null) == false)
                return _oddsChange;

            return _messageMapper.MapOddsChange<T>(_feedMessage, _rawMessage);
        }
    }
}
