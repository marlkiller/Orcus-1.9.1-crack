namespace Orcus.Shared.Commands.DropAndExecute
{
    public enum DropAndExecuteCommunication
    {
        InitializeFileTransfer,
        SendPackage,
        UploadCanceled,
        ExecuteFile,
        GetWindowUpdate,
        ResponseUploadCompleted,
        ResponseUploadFailed,
        ResponseFileNotFound,
        ResponseFileMightNotHaveExecutedSuccessfully,
        ResponseFileExecuted,
        ResponseFileExecutionFailed,
        ResponseBeginStreaming,
        ResponseWindowUpdate,
        WindowAction,
        StopExecution,
        StopStreaming,
        SwitchUserToHiddenDesktop,
        ResponseUserSwitched,
        SwitchUserBack,
        ResponseStopStreaming
    }
}