namespace Orcus.Commands.ReverseProxy.Args
{
    public class ReverseProxyDataReceivedEventArgs : ReverseProxyEventArgs
    {
        public ReverseProxyDataReceivedEventArgs(int connectionId, byte[] data) : base(connectionId)
        {
            Data = data;
        }

        public byte[] Data { get; }
    }
}