using System;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValueQWord : RegistryValue
    {
        public ulong Value { get; set; }
        public override RegistryValueKind ValueKind { get; } = RegistryValueKind.QWord;
        public override object ValueObject => Value;
    }
}