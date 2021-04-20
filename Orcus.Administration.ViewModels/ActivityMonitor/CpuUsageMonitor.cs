using System;
using System.Diagnostics;

namespace Orcus.Administration.ViewModels.ActivityMonitor
{
    public class CpuUsageMonitor : IDisposable
    {
        private readonly Process _process;
        private readonly TimeSpan _startTime;
        private DateTime _lastMonitorTime;
        private TimeSpan _oldCpuTime;

        public CpuUsageMonitor() : this(Process.GetCurrentProcess())
        {
        }

        public CpuUsageMonitor(Process process)
        {
            _process = process;
            _startTime = process.TotalProcessorTime;
        }

        public void Dispose()
        {
            _process.Dispose();
        }

        public double GetCurrentCpuUsage()
        {
            _process.Refresh();

            var newCpuTime = _process.TotalProcessorTime - _startTime;
            var value = (newCpuTime - _oldCpuTime).TotalSeconds/
                        (Environment.ProcessorCount*DateTime.UtcNow.Subtract(_lastMonitorTime).TotalSeconds);

            _lastMonitorTime = DateTime.UtcNow;
            _oldCpuTime = newCpuTime;

            return value;
        }
    }
}