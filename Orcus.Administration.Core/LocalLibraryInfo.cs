using Orcus.Shared.Connection;

namespace Orcus.Administration.Core
{
    public class LocalLibraryInfo
    {
        public LocalLibraryInfo(PortableLibrary library)
        {
            Library = library;
        }

        public PortableLibrary Library { get; }
        public int Length { get; set; }
        public byte[] Hash { get; set; }
        public string Path { get; set; }
    }
}