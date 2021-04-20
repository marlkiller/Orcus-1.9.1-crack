using System;

namespace Orcus.Shared.Commands.UninstallPrograms
{
    [Serializable]
    public class UninstallableProgram
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public int Size { get; set; }
        public string Version { get; set; }
        public UninstallProgramEntryLocation EntryLocation { get; set; }
        public int Id { get; set; }
        public byte[] IconData { get; set; }
    }
}