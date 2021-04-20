using System;
using System.Xml.Serialization;
using Orcus.Shared.DynamicCommands.TransmissionEvents;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     Event for the server when the dyanmic command should be sent to the clients
    /// </summary>
    [Serializable]
    [XmlInclude(typeof (DateTimeTransmissionEvent))]
    [XmlInclude(typeof (OnJoinTransmissionEvent))]
    [XmlInclude(typeof (RepeatingTransmissionEvent))]
    [XmlInclude(typeof (ImmediatelyTransmissionEvent))]
    [XmlInclude(typeof (EveryClientOnceTransmissionEvent))]
    public abstract class TransmissionEvent
    {
        /// <summary>
        ///     Types of all implementations of this class which are needed for serialization
        /// </summary>
        public static readonly Type[] AbstractTypes =
        {
            typeof (DateTimeTransmissionEvent), typeof (OnJoinTransmissionEvent),
            typeof (RepeatingTransmissionEvent), typeof (ImmediatelyTransmissionEvent),
            typeof (EveryClientOnceTransmissionEvent)
        };
    }
}