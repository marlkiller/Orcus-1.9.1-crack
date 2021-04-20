using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistrySubKeysPackage
    {
        public string Path { get; set; }
        public List<RegistrySubKey> RegistrySubKeys { get; set; }
        public RegistryHive RegistryHive { get; set; }
    }
}