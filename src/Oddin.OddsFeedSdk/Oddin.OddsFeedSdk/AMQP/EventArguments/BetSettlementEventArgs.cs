using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class BetSettlementEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly bet_settlement _feedMessage;
        private readonly IEnumerable<CultureInfo> _defaultCultures;
        private readonly byte[] _rawMessage;
        private readonly IBetSettlement<T> _betSettlement;

        internal BetSettlementEventArgs(
            IFeedMessageMapper messageMapper,
            bet_settlement feedMessage,
            IEnumerable<CultureInfo> defaultCultures,
            byte[] rawMessage)
        {
            if (messageMapper is null)
                throw new ArgumentNullException($"{nameof(messageMapper)}");

            if (feedMessage is null)
                throw new ArgumentNullException($"{nameof(feedMessage)}");


            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _defaultCultures = defaultCultures;
            _rawMessage = rawMessage;

            _betSettlement = GetBetSettlement();
        }

        public IBetSettlement<T> GetBetSettlement(CultureInfo culture = null)
        {
            if(_betSettlement is not null && culture is null)
                return _betSettlement;

            return _messageMapper.MapBetSettlement<T>(
                _feedMessage,
                culture is null
                    ? _defaultCultures
                    : new[] { culture },
                _rawMessage);
        }
    }
}
