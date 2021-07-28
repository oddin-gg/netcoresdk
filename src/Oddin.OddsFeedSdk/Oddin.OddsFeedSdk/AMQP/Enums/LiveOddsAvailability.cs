namespace Oddin.OddsFeedSdk.AMQP.Enums
{
    public enum LiveOddsAvailability
    {
        UNKNOWN,
        NOT_AVAILABLE,
        AVAILABLE
    }

    internal static class LiveOddsAvailabilityExtensions
    {
        internal static LiveOddsAvailability ParseToLiveOddsAvailability(this string source)
            => source switch
            {
                "not_available" => LiveOddsAvailability.NOT_AVAILABLE,
                "available" => LiveOddsAvailability.AVAILABLE,
                _ => LiveOddsAvailability.UNKNOWN
            };
    }
}
