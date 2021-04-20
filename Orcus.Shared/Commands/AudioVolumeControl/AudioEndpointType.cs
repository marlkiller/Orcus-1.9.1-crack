using System;

namespace Orcus.Shared.Commands.AudioVolumeControl
{
    [Serializable]
    public enum AudioEndpointType : byte
    {
        Render,
        Capture
    }
}