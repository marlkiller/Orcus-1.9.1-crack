using System;

namespace Orcus.Commands.Passwords.Utilities
{
    internal static class TimeHelper
    {
        public static long ToUnixTime(DateTime value)
        {
            var span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            return (long) span.TotalSeconds;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}