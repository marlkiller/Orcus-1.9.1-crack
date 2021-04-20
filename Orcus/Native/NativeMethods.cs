using System;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    internal static partial class NativeMethods
    {
        [DllImport("msvcrt.dll")]
        internal static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        [DllImport("Srclient.dll")]
        internal static extern int SRRemoveRestorePoint(uint index);

        [DllImport("ntdll.dll")]
        internal static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
            ref ProcessBasicInformation processInformation, int processInformationLength, out int returnLength);

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra,
            [Out] StringBuilder pszOut, ref uint pcchOut);


        [DllImport("winmm.dll")]
        internal static extern int mciSendString(string command, StringBuilder buffer, int bufferSize,
            IntPtr hwndCallback);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe IntPtr memcpy(void* dst, void* src, UIntPtr count);
    }
}