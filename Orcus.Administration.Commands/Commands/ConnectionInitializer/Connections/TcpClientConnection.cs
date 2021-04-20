using System;
using System.IO;
using System.Net.Sockets;

namespace Orcus.Administration.Commands.ConnectionInitializer.Connections
{
    public class TcpClientConnection : IConnection
    {
        private readonly TcpClient _tcpClient;
        private readonly BinaryReader _binaryReader;
        private readonly NetworkStream _networkStream;
        private readonly Func<int> _readInt32Delegate;

        public TcpClientConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _networkStream = tcpClient.GetStream();
            _binaryReader = new BinaryReader(_networkStream);

            _readInt32Delegate += _binaryReader.ReadInt32;
            _readInt32Delegate.BeginInvoke(Callback, null);
        }

        public void Dispose()
        {
            _networkStream.Close();
            _tcpClient.Close();
        }

        private void Callback(IAsyncResult ar)
        {
            byte[] data;
            try
            {
                var length = _readInt32Delegate.EndInvoke(ar);
                data = _binaryReader.ReadBytes(length);
            }
            catch (Exception)
            {
                //Disconnect?
                return;
            }

            DataReceived?.Invoke(this, new DataReceivedEventArgs(data));
            _readInt32Delegate.BeginInvoke(Callback, null);
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}