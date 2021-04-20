using System;
using Microsoft.Win32;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValueExpandString : RegistryValue
    {
        public string Value { get; set; }
        public override RegistryValueKind ValueKind { get; } = RegistryValueKind.ExpandString;
        public override object ValueObject => Value;
    }
}