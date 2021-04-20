namespace Orcus.Shared.Commands.ReverseProxy
{
    public enum ReverseProxyCommunication
    {
        Connect,
        SendData,
        Disconnect,
        ResponseStatusUpdate,
        ResponseData,
        ResponseDisconnected
    }
}