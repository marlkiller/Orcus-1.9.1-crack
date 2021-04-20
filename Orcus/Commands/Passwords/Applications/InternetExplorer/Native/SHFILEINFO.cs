using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    /// <summary>
    ///     Contains information about a file object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
    }
}