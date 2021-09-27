using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments
{
    public class FixtureChangeEventArgs<T> : EventArgs where T : ISportEvent
    {
        private readonly IFeedMessageMapper _messageMapper;
        private readonly fixture_change _feedMessage;
        private readonly IEnumerable<CultureInfo> _defaultCultures;
        private readonly byte[] _rawMessage;
        private readonly IFixtureChange<T> _fixtureChange;

        internal FixtureChangeEventArgs(
            IFeedMessageMapper messageMapper,
            fixture_change feedMessage,
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

            _fixtureChange = GetFixtureChange();
        }

        public IFixtureChange<T> GetFixtureChange(CultureInfo culture = null)
        {
            if (_fixtureChange is not null && culture is null)
                return _fixtureChange;

            return _messageMapper.MapFixtureChange<T>(
               _feedMessage,
               culture is null
                   ? _defaultCultures
                   : new[] { culture },
               _rawMessage);
        }
    }
}
