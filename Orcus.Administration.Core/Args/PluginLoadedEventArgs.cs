using System;

namespace Orcus.Administration.Core.Args
{
    public class PluginLoadedEventArgs : EventArgs
    {
        public PluginLoadedEventArgs(int clientId, Guid guid, string version, bool isLoaded)
        {
            ClientId = clientId;
            Guid = guid;
            Version = version;
            IsLoaded = isLoaded;
        }

        public int ClientId { get; }
        public Guid Guid { get; }
        public string Version { get; }
        public bool IsLoaded { get; }
    }
}