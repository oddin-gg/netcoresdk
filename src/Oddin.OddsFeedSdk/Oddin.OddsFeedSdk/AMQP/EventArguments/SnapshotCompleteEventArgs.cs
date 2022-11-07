using System;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Sessions;

namespace Oddin.OddsFeedSdk.AMQP.EventArguments;

public class SnapshotCompleteEventArgs : EventArgs
{
    internal readonly MessageInterest MessageInterest;
    internal readonly MessageTimestamp MessageTimestamp;
    internal readonly int ProducerId;
    internal readonly long RequestId;

    internal SnapshotCompleteEventArgs(
        snapshot_complete feedMessage,
        MessageInterest messageInterest)
    {
        ProducerId = feedMessage.product;
        RequestId = feedMessage.request_id;
        MessageInterest = messageInterest;

        MessageTimestamp = new MessageTimestamp(
            feedMessage.GeneratedAt,
            feedMessage.SentAt,
            feedMessage.ReceivedAt,
            Timestamp.Now());
    }
}