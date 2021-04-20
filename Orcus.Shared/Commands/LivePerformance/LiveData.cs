using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.LivePerformance
{
    [Serializable]
    public class LiveData
    {
        //CPU
        public byte InUse { get; set; }
        public uint ClockSpeed { get; set; }
        public int Processes { get; set; }
        public int Threads { get; set; }
        public int Handles { get; set; }
        public uint UpTimeSeconds { get; set; }

        //RAM
        public ulong UsedMemory { get; set; }

        //Ethernet
        public List<EthernetAdapterData> EthernetAdapterData { get; set; }
    }
}