namespace Orcus.Shared.Commands.DeviceManager
{
    public enum DeviceManagerCommunication
    {
        GetDevices,
        ResponseDevices,
        SetDeviceState,
        ErrorChangingDeviceState,
        DeviceStateChangedSuccessfully,
        ErrorDeviceNotFound
    }
}