using System;
using System.Net;
using System.Net.Sockets;
using NLog;

namespace Orcus.Server.Core
{
    public class Listener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly TcpListener _listener;

        public Listener(IPAddress ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
            _listener = new TcpListener(ipAddress, port);
        }

        public IPAddress IpAddress { get; }
        public int Port { get; }

        public event EventHandler<TcpClientConnectedEventArgs> Connect;

        public void Start()
        {
            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                Logger.Warn("Couldn't start listener ({0}:{1}): {2}", IpAddress, Port, ex.Message);
                return;
            }

            Logger.Info("Listener {0}:{1} started", IpAddress, Port);
            _listener.BeginAcceptSocket(EndAccept, null);
        }

        public void Stop()
        {
            _listener.Stop();
            Logger.Info("Listener {0}:{1} stopped", IpAddress, Port);
        }

        private void EndAccept(IAsyncResult asyncResult)
        {
            try
            {
                Logger.Debug("New connection in listener {0}:{1}", IpAddress, Port);
                Connect?.Invoke(this,
                    new TcpClientConnectedEventArgs(_listener.EndAcceptTcpClient(asyncResult)));
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                _listener.BeginAcceptTcpClient(EndAccept, null);
            }
            catch (InvalidOperationException)
            {
                //listener was stopped
            }
        }
    }
}