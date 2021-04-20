using System;
using System.Runtime.InteropServices;

namespace Orcus.Commands.RemoteDesktop.Capture.GDI
{
    public static class NativeMethods
    {
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight,
            [In] IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput,
            IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteDC([In] IntPtr hdc);
    }
}