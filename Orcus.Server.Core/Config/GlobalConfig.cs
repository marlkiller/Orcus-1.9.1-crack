using System;
using System.IO;

namespace Orcus.Server.Core.Config
{
    public class GlobalConfig
    {
        private const string ConfigFilename = "config.ini";

        private static GlobalConfig _current;
        public static GlobalConfig Current => _current ?? (_current = new GlobalConfig());

        public IniFile IniFile { get; }

        public GlobalConfig()
        {
            IniFile = new IniFile();
            Load();
        }

        private void Load()
        {
            if (File.Exists(ConfigFilename))
            {
                IniFile.Load(ConfigFilename);
                var version = int.Parse(IniFile.GetKeyValue("CONFIG_FILE", "Version"));
                if (version == 1)
                {
                    IniFile.SetKeyValue("CONFIG_FILE", "Version", "2");
                    IniFile.SetKeyValue("SERVER", "EnabledNamedPipe", "true");
                    IniFile.SetKeyValue("SERVER", "NamedPipeConnectionTimeout", "4000");
                    version = 2;
                }

                if (version == 2)
                {
                    IniFile.SetKeyValue("CONFIG_FILE", "Version", "3");
                    var dynamicCommandSection = IniFile.AddSection("DYNAMIC_COMMAND");
                    dynamicCommandSection.AddKey("MaxParallelPluginUploads").Value = "50";
                    dynamicCommandSection.AddKey("MaxPluginCacheSize").Value = "0";
                    version = 3;
                }

                IniFile.Save(ConfigFilename);
            }
            else
            {
                var configSection = IniFile.AddSection("CONFIG_FILE");
                configSection.AddKey("Version").Value = "2";

                var serverSection = IniFile.AddSection("SERVER");
                serverSection.AddKey("ConnectionTimeout").Value = "10000";
                serverSection.AddKey("CheckForDeadConnections").Value = "true";
                serverSection.AddKey("CheckForDeadConnectionsInterval").Value = "10000";
                serverSection.AddKey("CheckForDeadConnectionsTimeout").Value = "60000";
                serverSection.AddKey("CheckForDeadConnectionsRequestAnswer").Value = "20000";
                serverSection.AddKey("EnabledNamedPipe").Value = "true";
                serverSection.AddKey("NamedPipeConnectionTimeout").Value = "4000";

                var fileManagerSection = IniFile.AddSection("DATA_MANAGER");
                fileManagerSection.AddKey("WaitTimeout").Value = "30000";
                fileManagerSection.AddKey("MaxParallelTransfers").Value = "10";
                fileManagerSection.AddKey("CheckFileExists").Value = "false";
                fileManagerSection.AddKey("DownloadDataBuffer").Value = "4096";

                var geoIpSection = IniFile.AddSection("GEOIP_LOCATION");
                geoIpSection.AddKey("PatchDay").Value = new Random().Next(0, 8).ToString();
                geoIpSection.AddKey("Directory").Value = "GeoIp";
                geoIpSection.AddKey("UseServerIpAddressIfOutOfRange").Value = "true";
                geoIpSection.AddKey("RefreshServerIpPeriod").Value = "12";

                var dynamicCommandSection = IniFile.AddSection("DYNAMIC_COMMAND");
                dynamicCommandSection.AddKey("MaxParallelPluginUploads").Value = "50";
                dynamicCommandSection.AddKey("MaxPluginCacheSize").Value = "0";

                IniFile.Save(ConfigFilename);
            }
        }
    }
}