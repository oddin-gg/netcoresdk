using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments;

public class BetCancelEventArgs<T> : EventArgs where T : ISportEvent
{
    private readonly IBetCancel<T> _betCancel;
    private readonly IEnumerable<CultureInfo> _defaultCultures;
    private readonly bet_cancel _feedMessage;
    private readonly IFeedMessageMapper _messageMapper;
    private readonly byte[] _rawMessage;

    internal BetCancelEventArgs(
        IFeedMessageMapper messageMapper,
        bet_cancel feedMessage,
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

        _betCancel = GetBetCancel();
    }

    public IBetCancel<T> GetBetCancel(CultureInfo culture = null)
    {
        if (_betCancel is not null && culture is null)
            return _betCancel;

        return _messageMapper.MapBetCancel<T>(
            _feedMessage,
            culture is null
                ? _defaultCultures
                : new[] { culture },
            _rawMessage);
    }
}