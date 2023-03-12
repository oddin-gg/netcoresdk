using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.AMQP.Messages;

namespace Oddin.OddsFeedSdk.AMQP
{
    internal static class FeedMessageDeserializer
    {
        internal static bool TryDeserializeMessage(string message, out FeedMessageModel feedMessage)
        {
            if (XmlHelper.TryDeserialize(message, out alive aliveMessage))
            {
                feedMessage = aliveMessage;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out snapshot_complete snapshotComplete))
            {
                feedMessage = snapshotComplete;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out odds_change oddsChangeMessage))
            {
                feedMessage = oddsChangeMessage;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out bet_stop betStopMessage))
            {
                feedMessage = betStopMessage;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out bet_settlement betSettlement))
            {
                feedMessage = betSettlement;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out rollback_bet_settlement rollbackBetSettlement))
            {
                feedMessage = rollbackBetSettlement;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out rollback_bet_cancel rollbackBetCancel))
            {
                feedMessage = rollbackBetCancel;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out bet_cancel betCancel))
            {
                feedMessage = betCancel;
                return true;
            }

            if (XmlHelper.TryDeserialize(message, out fixture_change fixtureChange))
            {
                feedMessage = fixtureChange;
                return true;
            }

            feedMessage = null;
            return false;
        }
    }
}
