using Oddin.OddsFeedSdk.AMQP.Messages;

namespace Oddin.OddsFeedSdk.AMQP.Enums;

public enum EventStatus
{
    NotStarted = 0,
    Live = 1,
    Suspended = 2,
    Ended = 3,
    Finished = 4,
    Cancelled = 5,
    Abandoned = 6,
    Delayed = 7,
    Unknown = 8,
    Postponed = 9,
    Interrupted = 10
}

internal static class EventStatusExtenstions
{
    internal static EventStatus GetEventStatusFromApi(this string source)
        => source switch
        {
            "not_started" => EventStatus.NotStarted,
            "live" => EventStatus.Live,
            "suspended" => EventStatus.Suspended,
            "ended" => EventStatus.Ended,
            "closed" => EventStatus.Finished,
            "cancelled" => EventStatus.Cancelled,
            "abandoned" => EventStatus.Abandoned,
            "delayed" => EventStatus.Delayed,
            "unknown" => EventStatus.Unknown,
            "postponed" => EventStatus.Postponed,
            "interrupted" => EventStatus.Interrupted,
            _ => EventStatus.Unknown
        };

    internal static EventStatus GetEventStatusFromFeed(this eventStatus source)
        => source switch
        {
            eventStatus.NotStarted => EventStatus.NotStarted,
            eventStatus.Live => EventStatus.Live,
            eventStatus.Suspended => EventStatus.Suspended,
            eventStatus.Ended => EventStatus.Ended,
            eventStatus.Closed => EventStatus.Finished,
            eventStatus.Cancelled => EventStatus.Cancelled,
            eventStatus.Delayed => EventStatus.Delayed,
            eventStatus.Interrupted => EventStatus.Interrupted,
            eventStatus.Postponed => EventStatus.Postponed,
            eventStatus.Abandoned => EventStatus.Abandoned,
            _ => EventStatus.Unknown
        };
}