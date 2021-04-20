namespace Orcus.Shared.Commands.UninstallPrograms
{
    public enum UninstallProgramsCommunication : byte
    {
        ListInstalledPrograms,
        ResponseInstalledPrograms,
        UninstallProgram,
        ResponseProgramUninstallerStarted,
        ResponseUninstallFailed,
        ResponseEntryNotFound
    }
}