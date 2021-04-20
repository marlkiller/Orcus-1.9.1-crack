using System;

namespace Orcus.Shared.Client
{
    [Serializable]
    public class PluginResourceInfo
    {
        public string ResourceName { get; set; }
        public ResourceType ResourceType { get; set; }
        public Guid Guid { get; set; }
        public string PluginName { get; set; }
        public string PluginVersion { get; set; }
    }

    [Serializable]
    public enum ResourceType : byte
    {
        Command,
        ClientPlugin,
        FactoryCommand
    }
}