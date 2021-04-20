using System;
using System.Runtime.InteropServices;
using System.Text;
using Orcus.Native.Shell;
using ShellDll;
// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    internal static partial class NativeMethods
    {
        [DllImport("shell32.dll")]
        internal static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder,
            bool fCreate);

        [DllImport("shell32.dll")]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
            uint cbSizeFileInfo, ShellAPI.SHGFI uFlags);

        [DllImport("shell32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal static extern IntPtr SHGetLocalizedName(string pszPath, StringBuilder pszResModule, ref uint cch,
            out int pidsRes);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            // The following parameter is not used - binding context.
            IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);
    }
}