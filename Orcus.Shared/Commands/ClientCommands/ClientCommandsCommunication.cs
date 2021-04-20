namespace Orcus.Shared.Commands.ClientCommands
{
    public enum ClientCommandsCommunication
    {
        SendCommand,
        SendCommandWithPlugin,
        ResponseCommandSucceeded,
        ResponseCommandFailed,
        ResponseCommandMessage,
        CheckIsPluginAvailable,
        ResponseCheckPluginAvailable,
        ResponseFailedLoadingPlugin
    }
}