using System.Diagnostics;
using Orcus.Plugins;

namespace OrcusPluginStudio.Core.Test.ClientVirtualisation
{
    public class ToolBase : IToolBase
    {
        public IServicePipe ServicePipe { get; } = null;

        public void ExecuteFileAnonymously(string path, string arguments)
        {
            Process.Start(path, arguments);
        }

        public void ExecuteFileAnonymously(string path)
        {
            Process.Start(path);
        }
    }
}