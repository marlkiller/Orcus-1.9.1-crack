using System;

namespace Orcus.Shared.DynamicCommands.TransmissionEvents
{
    /// <summary>
    ///     Execute the command on every client within the targets once. The command will be executed on client join and to all
    ///     clients which are currently online.
    /// </summary>
    [Serializable]
    public class EveryClientOnceTransmissionEvent : TransmissionEvent
    {
    }
}