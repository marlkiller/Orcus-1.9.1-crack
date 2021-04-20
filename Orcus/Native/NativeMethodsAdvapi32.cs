using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    internal static partial class NativeMethods
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptAcquireContext(out IntPtr phProv, string pszContainer, string pszProvider,
            uint dwProvType, uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptCreateHash(IntPtr hProv, ALG.ALG_ID algid, IntPtr hKey, uint dwFlags,
            ref IntPtr phHash);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptHashData(IntPtr hHash, byte[] pbData, int dwDataLen, uint dwFlags);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptDestroyHash(IntPtr hHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptGetHashParam(IntPtr hHash, HashParameters dwParam, byte[] pbData,
            ref uint pdwDataLen, uint dwFlags);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        internal static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, uint ulOptions, uint samDesired,
            out IntPtr hkResult);

        [DllImport("advapi32")]
        internal static extern bool OpenProcessToken(
            IntPtr ProcessHandle, // handle to process
            int DesiredAccess, // desired access to process
            ref IntPtr TokenHandle // handle to open access token
        );

        [DllImport("advapi32", CharSet = CharSet.Auto)]
        internal static extern bool GetTokenInformation(
            IntPtr hToken,
            TOKEN_INFORMATION_CLASS tokenInfoClass,
            IntPtr TokenInformation,
            int tokeInfoLength,
            ref int reqLength
        );

        [DllImport("advapi32", CharSet = CharSet.Auto)]
        internal static extern bool ConvertSidToStringSid(
            IntPtr pSID,
            [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid
        );

        [DllImport("advapi32", CharSet = CharSet.Auto)]
        internal static extern bool ConvertStringSidToSid(
            [In, MarshalAs(UnmanagedType.LPTStr)] string pStringSid,
            ref IntPtr pSID
        );
    }
}