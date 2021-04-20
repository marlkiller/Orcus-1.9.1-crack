using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class ExecutableProperties
    {
        public bool IsSigned { get; set; }
        public bool IsAssembly { get; set; }
    }
}