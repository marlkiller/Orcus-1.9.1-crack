using System;

namespace Orcus.Shared.Commands.VoiceChat
{
    [Serializable]
    public class CaptureDeviceInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsDefault { get; set; }
    }
}