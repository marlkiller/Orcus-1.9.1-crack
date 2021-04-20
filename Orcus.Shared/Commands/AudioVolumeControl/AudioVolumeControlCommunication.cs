namespace Orcus.Shared.Commands.AudioVolumeControl
{
    public enum AudioVolumeControlCommunication : byte
    {
        GetDevices,
        SetDeviceMasterVolume,
        SetDeviceChannelVolume,
        ResponseDevices,
        ResponseVolumeSet,
        ResponseDeviceNotFound,
        ResponseChannelNotFound,
        ResponseNotSupported
    }
}