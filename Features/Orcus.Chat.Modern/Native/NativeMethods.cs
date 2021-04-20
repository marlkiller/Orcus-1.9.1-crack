using System;
using System.Runtime.InteropServices;

namespace Orcus.Chat.Modern.Native
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindow(string className, string windowText);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}