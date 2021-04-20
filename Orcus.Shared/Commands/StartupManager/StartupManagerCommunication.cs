namespace Orcus.Shared.Commands.StartupManager
{
    public enum StartupManagerCommunication : byte
    {
        GetAutostartEntries,
        RemoveAutostartEntry,
        ResponseAutostartEntries,
        EnableAutostartEntry,
        DisableAutostartEntry,
        ResponseAutostartEntryDisabled,
        ResponseAutostartEntryEnabled,
        ResponseAutostartEntryRemoved,
        ResponseAutostartChangingFailed
    }
}