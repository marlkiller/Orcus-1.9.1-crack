namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public enum FileProcessEntryState
    {
        Waiting,
        Preparing,
        Busy,
        Failed,
        Succeed,
        Canceled,
        FileNotFound,
        HashValuesNotMatch,
        InvalidFileLength,
        DirectoryNotFound,
        UnpackingDirectory
    }
}