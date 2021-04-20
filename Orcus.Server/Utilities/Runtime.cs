using System;

namespace Orcus.Server.Utilities
{
    internal static class Runtime
    {
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}