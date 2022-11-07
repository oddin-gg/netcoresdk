using System;

namespace Oddin.OddsFeedSdk.Common;

public static class Timestamp
{
    public static long ToEpochTimeMilliseconds(this DateTime dateTime) =>
        new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

    public static DateTime FromEpochTimeMilliseconds(this long timestamp) =>
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;

    public static long Now() => DateTime.UtcNow.ToEpochTimeMilliseconds();

    public static long FromSeconds(int seconds) => seconds * 1000L;

    public static long FromMinutes(int minutes) => minutes * 60 * 1000L;
}