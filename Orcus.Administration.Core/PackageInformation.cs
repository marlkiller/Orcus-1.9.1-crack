using System;

namespace Orcus.Administration.Core
{
    public class PackageInformation
    {
        public long Size { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsReceived { get; set; }
        public string Description { get; set; }
    }
}