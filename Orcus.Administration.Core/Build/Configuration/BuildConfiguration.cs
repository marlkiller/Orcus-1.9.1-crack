using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Orcus.Plugins;
using Orcus.Shared.Client;

namespace Orcus.Administration.Core.Build.Configuration
{
    [Serializable]
    public class BuildConfiguration
    {
        public List<ClientSetting> Settings { get; set; }
        public List<BuildConfigurationPlugin> Plugins { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }

        public string Export(bool formatted)
        {
            return JsonConvert.SerializeObject(this, formatted ? Formatting.Indented : Formatting.None);
        }

        public static BuildConfiguration Import(string source)
        {
            return JsonConvert.DeserializeObject<BuildConfiguration>(source);
        }
    }

    public class BuildConfigurationPlugin
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public PluginVersion Version { get; set; }
        public PluginSettingType Type { get; set; }
    }
}