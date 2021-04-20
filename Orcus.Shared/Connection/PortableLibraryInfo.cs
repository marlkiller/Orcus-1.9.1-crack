using System;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class PortableLibraryInfo
    {
        public PortableLibrary Library { get; set; }
        public int Length { get; set; }
    }
}