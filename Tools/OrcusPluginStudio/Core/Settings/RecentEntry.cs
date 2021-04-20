using System;

namespace OrcusPluginStudio.Core.Settings
{
    public class RecentEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime LastAccessTimestamp { get; set; }
    }
}