using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValuesPackage
    {
        public List<RegistryValue> Values { get; set; }
        public string Path { get; set; }
        public RegistryHive RegistryHive { get; set; }
    }
}