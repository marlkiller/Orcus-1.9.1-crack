using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.AudioVolumeControl
{
    [Serializable]
    public class AudioDevice
    {
        public float CurrentVolume { get; set; }
        public string Name { get; set; }
        public AudioEndpointType AudioEndpointType { get; set; }
        public bool IsDefault { get; set; }
        public int DeviceId { get; set; }
        public List<AudioChannel> AudioChannels { get; set; }
    }
}