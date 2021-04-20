using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.LivePerformance
{
    [Serializable]
    public class StaticPerformanceData
    {
        //CPU
        public string ProcessorName { get; set; }
        public uint MaxClockSpeed { get; set; }
        public uint Cores { get; set; }
        public uint LogicalProcessors { get; set; }
        public uint L1Cache { get; set; }
        public uint L2Cache { get; set; }
        public uint L3Cache { get; set; }

        //Memory
        public uint MemorySpeed { get; set; }
        public ushort TotalMemorySlots { get; set; }
        public int UsedMemorySlots { get; set; }
        public ulong TotalMemory { get; set; }

        //Ethernet
        public List<EthernetAdapter> EthernetAdapters { get; set; }
    }
}