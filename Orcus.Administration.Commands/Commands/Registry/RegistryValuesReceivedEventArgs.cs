using System;
using System.Collections.Generic;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Administration.Commands.Registry
{
    public class RegistryValuesReceivedEventArgs : EventArgs
    {
        public RegistryValuesReceivedEventArgs(string path, RegistryHive registryHive,
            List<RegistryValue> registryValues)
        {
            Path = path;
            RegistryHive = registryHive;
            RegistryValues = registryValues;
        }

        public string Path { get; }
        public RegistryHive RegistryHive { get; }
        public List<RegistryValue> RegistryValues { get; }
    }
}