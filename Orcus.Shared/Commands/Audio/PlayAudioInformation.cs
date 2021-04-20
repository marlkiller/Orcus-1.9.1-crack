using System;

namespace Orcus.Shared.Commands.Audio
{
    [Serializable]
    public class PlayAudioInformation
    {
        public byte[] AudioData { get; set; }
        public float Volume { get; set; }
        public string SoundOutId { get; set; }
    }
}