using System;

namespace Oddin.OddsFeedSdk.Common
{

    public static class Timestamp
    {

            public static long ToEpochTimeMilliseconds(this DateTime dateTime)
            {
                return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
            }

            public static DateTime FromEpochTimeMilliseconds(this long timestamp)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
            }

            public static long Now()
            {
                return  DateTime.UtcNow.ToEpochTimeMilliseconds();
            }

            public static long FromSeconds(int seconds)
            {
                return seconds * 1000L;
            }

            public static long FromMinutes(int minutes)
            {
                return minutes * 60 * 1000L;
            }
    }
}