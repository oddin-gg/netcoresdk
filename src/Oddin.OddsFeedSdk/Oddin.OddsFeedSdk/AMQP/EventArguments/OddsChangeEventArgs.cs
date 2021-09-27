using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class OddsChangeEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly odds_change _feedMessage;
        private readonly IEnumerable<CultureInfo> _defaultCultures;
        private readonly byte[] _rawMessage;
        private readonly IOddsChange<T> _oddsChange;

        internal OddsChangeEventArgs(
            IFeedMessageMapper messageMapper,
            odds_change feedMessage,
            IEnumerable<CultureInfo> defaultCultures,
            byte[] rawMessage)
        {
            if (messageMapper is null)
                throw new ArgumentNullException(nameof(messageMapper));

            if (feedMessage is null)
                throw new ArgumentNullException(nameof(feedMessage));

            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _defaultCultures = defaultCultures;
            _rawMessage = rawMessage;

            _oddsChange = GetOddsChange();
        }

        public IOddsChange<T> GetOddsChange(CultureInfo culture = null)
        {
            if (_oddsChange is not null && culture is null)
                return _oddsChange;

            return _messageMapper.MapOddsChange<T>(
                _feedMessage,
                culture is null
                    ? _defaultCultures
                    : new[] { culture },
                _rawMessage);
        }
    }
}
