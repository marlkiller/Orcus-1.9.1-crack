using System;
using System.Runtime.Serialization;
using Microsoft.Win32;

namespace Orcus.Shared.Commands.Registry
{
    [Serializable]
    [KnownType(typeof (RegistryValueBinary)), KnownType(typeof (RegistryValueDWord)),
     KnownType(typeof (RegistryValueExpandString)), KnownType(typeof (RegistryValueMultiString)),
     KnownType(typeof (RegistryValueQWord)), KnownType(typeof (RegistryValueString))]
    public abstract class RegistryValue
    {
        public string Key { get; set; }
        public abstract RegistryValueKind ValueKind { get; }
        public abstract object ValueObject { get; }

        public static Type[] RegistryValueTypes { get; } = {
            typeof (RegistryValueBinary), typeof (RegistryValueDWord),
            typeof (RegistryValueExpandString), typeof (RegistryValueMultiString),
            typeof (RegistryValueQWord), typeof (RegistryValueString), typeof (RegistryValueUnknown)
        };
    }
}