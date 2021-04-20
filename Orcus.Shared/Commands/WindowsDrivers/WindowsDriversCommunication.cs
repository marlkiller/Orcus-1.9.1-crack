namespace Orcus.Shared.Commands.WindowsDrivers
{
    public enum WindowsDriversCommunication
    {
        GetDriversFile,
        GetAllDriversFiles,
        ResponseDriversFileContent,
        ChangeDriversFile,
        ResponseChangedSuccessfully,
        ResponseChangingFailed
    }

    public enum WindowsDriversFile
    {
        Hosts,
        Networks,
        Protocol,
        Services
    }
}