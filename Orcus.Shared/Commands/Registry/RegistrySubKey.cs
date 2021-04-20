using System;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistrySubKey
    {
        public string Name { get; set; }
        public bool IsEmpty { get; set; }
    }
}