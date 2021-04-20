using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Orcus.Shared.Core;

namespace Orcus.Shared.Client
{
    [Serializable, XmlInclude(typeof(PluginSetting))]
    public class ClientSetting
    {
        [XmlAttribute]
        public string SettingsType { get; set; }

        public List<PropertyNameValue> Properties { get; set; }
    }
}