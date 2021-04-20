using System;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistrySubKeyAction
    {
        public string Path { get; set; }
        public RegistryHive RegistryHive { get; set; }
    }
}