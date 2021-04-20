using System.Runtime.InteropServices;

namespace Orcus.Server.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemTime
    {
        public short Year;
        public short Month;
        public short DayOfWeek;
        public short Day;
        public short Hour;
        public short Minute;
        public short Second;
        public short Milliseconds;
    }
}