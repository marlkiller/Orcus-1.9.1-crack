namespace Orcus.Shared.Commands.FileExplorer
{
    public enum FileExplorerCommunication : byte
    {
        SendDtpPackage,
        ResponseDtpPackage,
        SendUploadPackage,
        ResponsePackagingDirectory,
        ResponseCopyingFile,
        ResponseDownloadPackage,
        ResponseDownloadFailed,
        ResponseProcessingEntryChanged,
        ResponseProcessingEntryAdded
    }
}