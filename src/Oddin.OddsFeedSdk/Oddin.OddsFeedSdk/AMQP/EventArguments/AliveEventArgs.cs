using System;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Sessions;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments;

public class AliveEventArgs : EventArgs
{
    internal readonly bool IsSubscribed;
    internal readonly MessageInterest MessageInterest;
    internal readonly MessageTimestamp MessageTimestamp;
    internal readonly int ProducerId;

    internal AliveEventArgs(
        alive feedMessage,
        MessageInterest messageInterest)
    {
        ProducerId = feedMessage.product;
        MessageInterest = messageInterest;

        MessageTimestamp = new MessageTimestamp(
            feedMessage.GeneratedAt,
            feedMessage.SentAt,
            feedMessage.ReceivedAt,
            Timestamp.Now());

        IsSubscribed = feedMessage.subscribed == 1;
    }
}