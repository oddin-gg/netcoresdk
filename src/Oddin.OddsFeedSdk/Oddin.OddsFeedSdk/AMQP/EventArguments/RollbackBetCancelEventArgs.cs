using System;
using System.Collections.Generic;
using System.Globalization;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments;

public class RollbackBetCancelEventArgs<T> : EventArgs where T : ISportEvent
{
    private readonly IEnumerable<CultureInfo> _defaultCultures;
    private readonly rollback_bet_cancel _feedMessage;
    private readonly IFeedMessageMapper _messageMapper;
    private readonly byte[] _rawMessage;
    private readonly IRollbackBetCancel<T> _rollbackBetCancel;

    internal RollbackBetCancelEventArgs(
        IFeedMessageMapper messageMapper,
        rollback_bet_cancel feedMessage,
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

        _rollbackBetCancel = GetRollbackBetCancel();
    }

    public IRollbackBetCancel<T> GetRollbackBetCancel(CultureInfo culture = null)
    {
        if (_rollbackBetCancel is not null && culture is null)
            return _rollbackBetCancel;

        return _messageMapper.MapRollbackBetCancel<T>(
            _feedMessage,
            culture is null
                ? _defaultCultures
                : new[] { culture },
            _rawMessage);
    }
}