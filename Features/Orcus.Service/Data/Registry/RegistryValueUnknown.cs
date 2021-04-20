using System;
using Microsoft.Win32;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    public class RegistryValueUnknown : RegistryValue
    {
        public override RegistryValueKind ValueKind { get; } = RegistryValueKind.Unknown;
        public override object ValueObject { get; } = null;
    }
}