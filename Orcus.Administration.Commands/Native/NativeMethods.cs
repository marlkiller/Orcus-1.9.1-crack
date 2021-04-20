using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Orcus.Administration.Commands.Native
{
    internal class NativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        internal static extern void CopyMemory(IntPtr dest, IntPtr source, int Length);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryExW")]
        internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll", EntryPoint = "LoadStringW", CallingConvention = CallingConvention.Winapi,
            CharSet = CharSet.Unicode)]
        internal static extern int LoadString(IntPtr hModule, int resourceID, StringBuilder resourceValue, int len);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int PSGetPropertyDescription(
            ref PropertyKey propkey,
            ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPropertyDescription ppv
            );

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        internal static extern int SendARP(uint destIpAddress, uint srcIpAddress, byte[] macAddress,
            ref int macAddressLength);

        /// <summary>
        ///     The MapVirtualKey function translates (maps) a virtual-key code into a scan
        ///     code or character value, or translates a scan code into a virtual-key code
        /// </summary>
        /// <param name="uCode">
        ///     [in] Specifies the virtual-key code or scan code for a key.
        ///     How this value is interpreted depends on the value of the uMapType parameter
        /// </param>
        /// <param name="uMapType">
        ///     [in] Specifies the translation to perform. The value of this
        ///     parameter depends on the value of the uCode parameter.
        /// </param>
        /// <returns>
        ///     Either a scan code, a virtual-key code, or a character value, depending on
        ///     the value of uCode and uMapType. If there is no translation, the return value is zero
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern int MapVirtualKey(uint uCode, MapVirtualKeyMapTypes uMapType);
    }
}