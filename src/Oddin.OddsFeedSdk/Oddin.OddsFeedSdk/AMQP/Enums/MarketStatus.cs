namespace Oddin.OddsFeedSdk.AMQP.Enums
{
    public enum MarketStatus
    {
        ACTIVE = 1,

        DEACTIVATED = 0,

        // This field is deprecated, please use DEACTIVATED
        INACTIVE = DEACTIVATED,

        SUSPENDED = -1,

        HANDED_OVER = -2,

        SETTLED = -3,

        CANCELLED = -4
    }
}
