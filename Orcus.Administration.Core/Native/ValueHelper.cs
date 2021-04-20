namespace Orcus.Administration.Core.Native
{
    public static class ValueHelper
    {
        public static uint MakeWord(byte low, byte high)
        {
            return ((uint) high << 8) | low;
        }
    }
}