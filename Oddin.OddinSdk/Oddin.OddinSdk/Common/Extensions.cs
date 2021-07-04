using System;
using System.Collections.Generic;
using System.Text;

namespace Oddin.OddinSdk.Common
{
    internal static class Extensions
    {
        public static long ToEpochTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
    }
}
