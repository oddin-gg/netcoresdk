namespace Oddin.OddsFeedSdk.AMQP.Messages;

public enum MessageType
{
    UNKNOWN,

    PRODUCER_DOWN,

    SNAPSHOT_COMPLETE,

    ALIVE,

    FIXTURE_CHANGE,

    BET_STOP,

    BET_CANCEL,

    ROLLBACK_BET_CANCEL,

    BET_SETTLEMENT,

    ROLLBACK_BET_SETTLEMENT,

    ODDS_CHANGE
}