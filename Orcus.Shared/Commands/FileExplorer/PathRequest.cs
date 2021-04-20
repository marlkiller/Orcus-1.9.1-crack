using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class PathRequest
    {
        public bool DirectoriesOnly { get; set; }
        public string Path { get; set; }
    }
}