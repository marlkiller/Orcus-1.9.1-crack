using System;

namespace Orcus.Shared.Commands.Audio
{
    [Serializable]
    public class SoundOutDevice
    {
        public bool IsDefault { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}