using System;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class VideoCardInfo
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public string VideoModeDescription { get; set; }
        public string VideoProcessor { get; set; }
        public int MaxRefreshRate { get; set; }
        public string VideoArchitecture { get; set; }
        public string VideoMemoryType { get; set; }
    }
}