using System;

namespace Orcus.Shared.Commands.AudioVolumeControl
{
    [Serializable]
    public class AudioChannel
    {
        public int ChannelIndex { get; set; }
        public float Volume { get; set; }
    }
}