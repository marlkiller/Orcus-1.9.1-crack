using System;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryCreateValuePackage
    {
        public RegistryHive RegistryHive { get; set; }
        public string Path { get; set; }
        public RegistryValue RegistryValue { get; set; }
    }
}