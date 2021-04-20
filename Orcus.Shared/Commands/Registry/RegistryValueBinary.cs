using System;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValueBinary : RegistryValue
    {
        public byte[] Value { get; set; }
        public override RegistryValueKind ValueKind { get; } = RegistryValueKind.Binary;
        public override object ValueObject => Value;
    }
}