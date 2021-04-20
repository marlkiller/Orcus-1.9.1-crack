using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BlueScreenTrigger
{
    internal static class Program
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass,
            ref int processInformation, int processInformationLength);

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            int isCritical = 1; // we want this to be a Critical Process
            int BreakOnTermination = 0x1D; // value for BreakOnTermination (flag)

            Process.EnterDebugMode(); //acquire Debug Privileges

            // setting the BreakOnTermination = 1 for the current process
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof (int));
            Environment.Exit(0);
        }
    }
}