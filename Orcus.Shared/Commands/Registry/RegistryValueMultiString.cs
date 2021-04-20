using System;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValueMultiString : RegistryValue
    {
        public string[] Value { get; set; }
        public override RegistryValueKind ValueKind { get; } = RegistryValueKind.MultiString;
        public override object ValueObject => Value;
    }
}