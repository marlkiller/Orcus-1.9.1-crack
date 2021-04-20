using Orcus.Shared.Commands.Registry;

namespace Orcus.Administration.Commands.Registry
{
    public class AdvancedRegistrySubKey : RegistrySubKey
    {
        public RegistryHive RegistryHive { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }
    }
}