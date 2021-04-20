namespace Orcus.Shared.Commands.SystemRestore
{
    public enum SystemRestoreCommunication
    {
        GetRestorePoints,
        RestorePoint,
        RemoveRestorePoint,
        CreateRestorePoint,
        ResponseRestorePoints,
        ResponseNoAccess,
        ResponseBeginRestore,
        ResponseRestoreSucceed,
        ResponseRestoreFailed,
        ResponseRemoveSucceed,
        ResponseRemoveFailed,
        ResponseCreatingRestorePoint,
        ResponseCreateRestorePointSucceed,
        ResponseCreateRestorePointFailed
    }
}