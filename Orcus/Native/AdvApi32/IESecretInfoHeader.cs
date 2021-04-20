using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    // IE Autocomplete Secret Data structures decoded by Nagareshwar
    //
    //One Secret Info header specifying number of secret strings
    [StructLayout(LayoutKind.Sequential)]
    internal struct IESecretInfoHeader
    {
        public uint dwIdHeader; // value - 57 49 43 4B
        public uint dwSize; // size of this header....24 bytes
        public uint dwTotalSecrets; // divide this by 2 to get actual website entries
        public uint unknown;
        public uint id4; // value - 01 00 00 00
        public uint unknownZero;
    }
}