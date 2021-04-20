using System;
using System.Threading;

namespace Orcus.Utilities
{
    public static class TimeoutEx
    {
        public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);
    }
}