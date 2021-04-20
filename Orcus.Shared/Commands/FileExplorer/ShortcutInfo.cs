using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class ShortcutInfo
    {
        public string TargetLocation { get; set; }
        public string Description { get; set; }
        public string WorkingDirectory { get; set; }
        public string IconPath { get; set; }
        public int IconIndex { get; set; }
        public short Hotkey { get; set; }
    }
}