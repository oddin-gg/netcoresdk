using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Messages;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.AMQP.EventArguments
{
    public class BetSettlementEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly bet_settlement _feedMessage;
        private readonly byte[] _rawMessage;
        private readonly IBetStop<T> _betStop;

        internal BetSettlementEventArgs(IFeedMessageMapper messageMapper, bet_settlement feedMessage, byte[] rawMessage)
        {
            if (messageMapper is null)
                throw new ArgumentNullException($"{nameof(messageMapper)}");

            if (feedMessage is null)
                throw new ArgumentNullException($"{nameof(feedMessage)}");


            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _rawMessage = rawMessage;

            _betStop = GetBetSettlement();
        }

        public IBetStop<T> GetBetSettlement()
        {
            if ((_betStop is null) == false)
                return _betStop;

            return _messageMapper.MapBetSettlement<T>(_feedMessage, _rawMessage);
        }
    }
}
