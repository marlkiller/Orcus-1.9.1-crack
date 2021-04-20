using System;
using System.Xml.Serialization;

namespace Orcus.Shared.Client
{
    [Serializable]
    public class PluginSetting : ClientSetting
    {
        [XmlAttribute]
        public Guid PluginId { get; set; }

        [XmlAttribute]
        public PluginSettingType PluginType { get; set; }
    }

    public enum PluginSettingType
    {
        ClientPlugin,
        BuildPlugin
    }
}