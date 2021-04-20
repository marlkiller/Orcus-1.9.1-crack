using System;

namespace Orcus.Shared.Commands.UninstallPrograms
{
    [Serializable]
    public enum UninstallProgramEntryLocation : byte
    {
        LocalMachine,
        CurrentUser,
        LocalMachineWow6432Node
    }
}