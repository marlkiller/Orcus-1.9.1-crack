using System;

namespace Orcus.Server.Core.DynamicCommands
{
    public class DynamicCommandPluginCacheItem
    {
        public DynamicCommandPluginCacheItem(string filename, byte[] data)
        {
            Filename = filename;
            Data = data;
            LastUsage = DateTime.UtcNow;
        }

        public string Filename { get; }
        public byte[] Data { get; }
        public DateTime LastUsage { get; set; }
    }
}