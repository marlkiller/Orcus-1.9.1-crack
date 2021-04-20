using System;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class ProcessorInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClockSpeed { get; set; }
        public uint Cores { get; set; }
        public int LogicalProcessors { get; set; }
        public string Architecture { get; set; }
        public string L2CacheSize { get; set; }
        public string L3CacheSize { get; set; }
        public string ManufactureId { get; set; }
        public string DeviceId { get; set; }
        public string ProcessorId { get; set; }
        public string ProcessorType { get; set; }
        public string ExternalClockSpeed { get; set; }
        public int Revision { get; set; }
    }
}