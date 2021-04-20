using System.Runtime.InteropServices;

namespace Orcus.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TOKEN_USER
    {
        public _SID_AND_ATTRIBUTES User;
    }
}