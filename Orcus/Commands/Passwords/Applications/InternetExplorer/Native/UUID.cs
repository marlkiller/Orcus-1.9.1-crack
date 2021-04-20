using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UUID
    {
        public int Data1;
        public short Data2;
        public short Data3;
        public byte[] Data4;
    }
}