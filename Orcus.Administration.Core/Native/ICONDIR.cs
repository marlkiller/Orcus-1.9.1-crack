using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Administration.Core.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONDIR
    {
        // Reserved, must be 0
        public ushort Reserved;
        // Resource type, 1 for icons.
        public ushort Type;
        // How many images.
        public ushort Count;
        // The native structure has an array of ICONDIRENTRYs as a final field.
    }
}