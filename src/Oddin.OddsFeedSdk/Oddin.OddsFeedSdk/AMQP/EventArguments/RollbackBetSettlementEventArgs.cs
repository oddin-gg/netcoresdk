using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class RollbackBetSettlementEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly rollback_bet_settlement _feedMessage;
        private readonly IEnumerable<CultureInfo> _defaultCultures;
        private readonly byte[] _rawMessage;
        private readonly IRollbackBetSettlement<T> _rollbackBetSettlement;

        internal RollbackBetSettlementEventArgs(
            IFeedMessageMapper messageMapper,
            rollback_bet_settlement feedMessage,
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

            _rollbackBetSettlement = GetRollbackBetSettlement();
        }

        public IRollbackBetSettlement<T> GetRollbackBetSettlement(CultureInfo culture = null)
        {
            if(_rollbackBetSettlement is not null && culture is null)
                return _rollbackBetSettlement;

            return _messageMapper.MapRollbackBetSettlement<T>(
                _feedMessage,
                culture is null
                    ? _defaultCultures
                    : new[] { culture },
                _rawMessage);
        }
    }
}
