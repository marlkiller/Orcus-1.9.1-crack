using System;
using System.Runtime.InteropServices;

namespace Orcus.Server.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CryptoApiBlob
    {
        public int DataLength;
        public IntPtr Data;

        public CryptoApiBlob(int dataLength, IntPtr data)
        {
            DataLength = dataLength;
            Data = data;
        }
    }
}