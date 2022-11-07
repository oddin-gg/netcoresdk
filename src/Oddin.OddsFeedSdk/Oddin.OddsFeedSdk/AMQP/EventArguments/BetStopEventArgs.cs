using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments;

public class BetStopEventArgs<T> : EventArgs where T : ISportEvent
{
    private readonly IBetStop<T> _betStop;
    private readonly IEnumerable<CultureInfo> _defaultCultures;
    private readonly bet_stop _feedMessage;
    private readonly IFeedMessageMapper _messageMapper;
    private readonly byte[] _rawMessage;

    internal BetStopEventArgs(
        IFeedMessageMapper messageMapper,
        bet_stop feedMessage,
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

        _betStop = GetBetStop();
    }

    public IBetStop<T> GetBetStop(CultureInfo culture = null)
    {
        if (_betStop is not null && culture is null)
            return _betStop;

        return _messageMapper.MapBetStop<T>(
            _feedMessage,
            culture is null
                ? _defaultCultures
                : new[] { culture },
            _rawMessage);
    }
}