using System;

namespace Orcus.Shared.Commands.VoiceChat
{
    [Serializable]
    public class VoiceChatBeginCaptureInfo
    {
        public string DeviceId { get; set; }
        public int Application { get; set; }
        public int Bitrate { get; set; }
    }
}