using System;
using System.Diagnostics;
using Orcus.Administration.Core.Native;

namespace Orcus.Administration.Core.Utilities
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
                    IntPtr child = NativeMethods.FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
                    NativeMethods.SendMessage(child, 0x000C, 0, message);
                }
            }
        }
    }
}