using System.Runtime.InteropServices;

namespace Orcus.Administration.Core.Native
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