using System;

namespace Orcus.Shared.Commands.LivePerformance
{
    [Serializable]
    public class EthernetAdapterData
    {
        public string Name { get; set; }
        public float BytesReceive { get; set; }
        public float BytesSend { get; set; }
    }
}