using System.Reflection;
using Orcus.Plugins;

namespace OrcusPluginStudio.Core.Test.ClientVirtualisation
{
    public class ClientStartupInformation : IClientStartup
    {
        public bool IsAdministrator { get; } = false;
        public string ClientPath { get; } = Assembly.GetExecutingAssembly().Location;
        public bool IsInstalled { get; } = false;
    }
}