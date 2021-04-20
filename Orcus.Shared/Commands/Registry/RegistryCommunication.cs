namespace Orcus.Shared.Commands.Registry
{
    public enum RegistryCommunication
    {
        GetRegistrySubKeys,
        GetRegistryValues,
        CreateSubKey,
        CreateValue,
        DeleteSubKey,
        DeleteValue,
        ResponseRegistrySubKeys,
        ResponseRegistryValues,
        ResponseSubKeyDeleted,
        PermissionsDeniedError,
        Error,
        ResponseSubKeyCreated,
        ResponseValueCreated,
        ResponseValueDeleted
    }
}