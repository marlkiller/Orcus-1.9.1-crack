using System;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class Screen
    {
        public string Resolution { get; set; }
        public bool IsPrimary { get; set; }
        public int BitsPerPixel { get; set; }
        public string DeviceName { get; set; }
    }
}