using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    internal static partial class NativeMethods
    {
        [DllImport("kernel32")]
        internal static extern ulong GetTickCount64();

        [DllImport("kernel32.dll")]
        internal static extern uint GetTickCount();

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        internal static extern int GetSystemDefaultLCID();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern bool CreateProcess(
            [MarshalAs(UnmanagedType.LPTStr)] string lpApplicationName, // LPCTSTR
            StringBuilder lpCommandLine, // LPTSTR - note: CreateProcess might insert a null somewhere in this string
            IntPtr lpProcessAttributes, // LPSECURITY_ATTRIBUTES
            IntPtr lpThreadAttributes, // LPSECURITY_ATTRIBUTES
            bool bInheritHandles, // BOOL
            int dwCreationFlags, // DWORD
            IntPtr lpEnvironment, // LPVOID
            [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, // LPCTSTR
            [In] ref STARTUPINFO lpStartupInfo, // LPSTARTUPINFO
            out PROCESS_INFORMATION lpProcessInformation // LPPROCESS_INFORMATION
        );

        [DllImport("kernel32.dll")]
        internal static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport("kernel32.dll")]
        internal static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        internal static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        internal static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32")]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        internal static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        internal static extern int GetDiskFreeSpaceW([In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
            out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters,
            out uint lpTotalNumberOfClusters);
    }
}