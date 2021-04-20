using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Administration.Commands.Registry
{
    public class SubKeysReceivedEventArgs : EventArgs
    {
        public SubKeysReceivedEventArgs(string path, List<RegistrySubKey> registrySubKeys, RegistryHive registryHive)
        {
            Path = path;
            RegistrySubKeys =
                registrySubKeys.Select(
                    x =>
                        new AdvancedRegistrySubKey
                        {
                            IsEmpty = x.IsEmpty,
                            Name = x.Name,
                            Path = path == "" ? x.Name : path + "\\" + x.Name,
                            RegistryHive = registryHive
                        }).ToList();
            RegistryHive = registryHive;
        }

        public string Path { get; }
        public List<AdvancedRegistrySubKey> RegistrySubKeys { get; }
        public RegistryHive RegistryHive { get; }
    }
}