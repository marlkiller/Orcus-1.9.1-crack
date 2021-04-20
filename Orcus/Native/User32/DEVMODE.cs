using System;
using System.Runtime.InteropServices;

namespace Orcus.Native.Display
{
    // See: https://msdn.microsoft.com/en-us/library/windows/desktop/dd183565(v=vs.85).aspx
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    internal struct DEVMODE
    {
        public const int CCHDEVICENAME = 32;
        public const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)] [FieldOffset(0)] public string dmDeviceName;
        [FieldOffset(32)] public Int16 dmSpecVersion;
        [FieldOffset(34)] public Int16 dmDriverVersion;
        [FieldOffset(36)] public Int16 dmSize;
        [FieldOffset(38)] public Int16 dmDriverExtra;
        [FieldOffset(40)] public DM dmFields;

        [FieldOffset(44)] Int16 dmOrientation;
        [FieldOffset(46)] Int16 dmPaperSize;
        [FieldOffset(48)] Int16 dmPaperLength;
        [FieldOffset(50)] Int16 dmPaperWidth;
        [FieldOffset(52)] Int16 dmScale;
        [FieldOffset(54)] Int16 dmCopies;
        [FieldOffset(56)] Int16 dmDefaultSource;
        [FieldOffset(58)] Int16 dmPrintQuality;

        [FieldOffset(44)] public Native.POINTL dmPosition;
        [FieldOffset(52)] public Int32 dmDisplayOrientation;
        [FieldOffset(56)] public Int32 dmDisplayFixedOutput;

        [FieldOffset(60)] public short dmColor;
        [FieldOffset(62)] public short dmDuplex;
        [FieldOffset(64)] public short dmYResolution;
        [FieldOffset(66)] public short dmTTOption;
        [FieldOffset(68)] public short dmCollate;
        [FieldOffset(72)] [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)] public string dmFormName;
        [FieldOffset(102)] public Int16 dmLogPixels;
        [FieldOffset(104)] public Int32 dmBitsPerPel;
        [FieldOffset(108)] public Int32 dmPelsWidth;
        [FieldOffset(112)] public Int32 dmPelsHeight;
        [FieldOffset(116)] public Int32 dmDisplayFlags;
        [FieldOffset(116)] public Int32 dmNup;
        [FieldOffset(120)] public Int32 dmDisplayFrequency;
    }
}