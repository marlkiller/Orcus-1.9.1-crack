using System;
using Orcus.Chat.Console.Native;

namespace Orcus.Chat.Console.Utilities
{
    public class ConsoleHelper
    {
        public static void DisableClose()
        {
            NativeMethods.DeleteMenu(NativeMethods.GetSystemMenu(NativeMethods.GetConsoleWindow(), false), SC_CLOSE,
                MF_BYCOMMAND);
        }

        public static string ReadLineWithoutShowing()
        {
            string s = System.Console.ReadLine();
            if (s != null)
            {
                var linesOfInput = 1 + s.Length/System.Console.BufferWidth;
                //Move cursor to just before the input just entered
                System.Console.CursorTop -= linesOfInput;
                System.Console.CursorLeft = 0;
                //blank out the content that was just entered
                System.Console.WriteLine(new string(' ', s.Length));
                //move the cursor to just before the input was just entered
                System.Console.CursorTop -= linesOfInput;
                System.Console.CursorLeft = 0;
            }
            return s;
        }

        public static void ShowConsoleWindow()
        {
            NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), SW_SHOW);
        }

        public static void HideConsoleWindow()
        {
            NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), SW_HIDE);
        }

        public static void SetTopMost()
        {
            NativeMethods.SetWindowPos(NativeMethods.GetConsoleWindow(),
                new IntPtr(HWND_TOPMOST),
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE);
        }

        // ReSharper disable InconsistentNaming
        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        // ReSharper restore InconsistentNaming
    }
}