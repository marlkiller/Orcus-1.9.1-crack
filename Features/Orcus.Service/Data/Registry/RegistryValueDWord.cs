using System;
using Microsoft.Win32;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValueDWord : RegistryValue
    {
        public uint Value { get; set; }
        public override RegistryValueKind ValueKind { get; } = RegistryValueKind.DWord;
        public override object ValueObject => Value;
    }
}