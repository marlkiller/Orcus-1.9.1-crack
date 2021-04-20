using System;

namespace Orcus.Shared.DynamicCommands.TransmissionEvents
{
    /// <summary>
    ///     Just execute the command immediately to all clients which are online
    /// </summary>
    [Serializable]
    public class ImmediatelyTransmissionEvent : TransmissionEvent
    {
    }
}