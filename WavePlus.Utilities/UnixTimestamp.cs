using System;

namespace Plus.Utilities
{
    internal static class UnixTimestamp
    {
        public static double GetNow()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
            return ts.TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(double timestamp)
        {
            DateTime dt = new(1970, 1, 1, 0, 0, 0, 0);
            dt = dt.AddSeconds(timestamp);
            return dt;
        }
    }
}