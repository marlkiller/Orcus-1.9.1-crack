namespace Orcus.Shared.Commands.Console
{
    public enum ConsoleCommunication : byte
    {
        SendStart,
        SendStop,
        SendCommand,
        ResponseNewLine,
        ResponseConsoleOpen,
        ResponseConsoleClosed,
        OpenConsoleInPath
    }
}