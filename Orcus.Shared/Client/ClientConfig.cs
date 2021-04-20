using System;
using System.Collections.Generic;

namespace Orcus.Shared.Client
{
    [Serializable]
    public class ClientConfig
    {
        public List<PluginResourceInfo> PluginResources { get; set; }
        public List<ClientSetting> Settings { get; set; }
    }
}