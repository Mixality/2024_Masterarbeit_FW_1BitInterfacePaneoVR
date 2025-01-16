using System;

namespace Utilities
{
    public static class TimeUtils
    {
        public static long GetUnixTimestamp(DateTime date)
        {
            DateTime zero = new DateTime(1970, 1, 1);
            TimeSpan span = date.Subtract(zero);

            return (long)span.TotalMilliseconds;
        }

        public static long GetUnixTimestamp()
        {
            return GetUnixTimestamp(DateTime.UtcNow);
        }
    }
}