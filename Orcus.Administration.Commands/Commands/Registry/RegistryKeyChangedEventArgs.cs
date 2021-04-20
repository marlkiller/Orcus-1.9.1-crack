using System;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Administration.Commands.Registry
{
    public class RegistryKeyChangedEventArgs : EventArgs
    {
        public RegistryKeyChangedEventArgs(string path, RegistryHive registryHive, string relativePath)
        {
            Path = path;
            RegistryHive = registryHive;
            RelativePath = relativePath;
        }

        public string Path { get; }
        public RegistryHive RegistryHive { get; }
        public string RelativePath { get; }
    }
}