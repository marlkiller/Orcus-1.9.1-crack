using System;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValueString : RegistryValue
    {
        public string Value { get; set; }
        public override RegistryValueKind ValueKind { get; } = RegistryValueKind.String;
        public override object ValueObject => Value;
    }
}