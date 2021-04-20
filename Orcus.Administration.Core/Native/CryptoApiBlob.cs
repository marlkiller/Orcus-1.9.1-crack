using System;
using System.Runtime.InteropServices;

namespace Orcus.Administration.Core.Native
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