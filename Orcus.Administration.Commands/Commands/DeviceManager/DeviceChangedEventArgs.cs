using System;

namespace Orcus.Administration.Commands.DeviceManager
{
    public class DeviceChangedEventArgs : EventArgs
    {
        public DeviceChangedEventArgs(string hardwareId, bool newState)
        {
            HardwareId = hardwareId;
            NewState = newState;
        }

        public string HardwareId { get; }
        public bool NewState { get; }
    }
}