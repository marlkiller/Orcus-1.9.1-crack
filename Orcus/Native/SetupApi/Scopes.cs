using System;

namespace Orcus.Native
{
    [Flags]
    public enum Scopes
    {
        Global = 1,
        ConfigSpecific = 2,
        ConfigGeneral = 4
    }
}