using Orcus.Plugins;
#if !DEBUG
using System;
using System.ServiceModel;
#endif

namespace Orcus.Service
{
    internal class ServiceConnection
    {
        private static ServiceConnection _serviceConnection;

        private ServiceConnection()
        {
            Program.WriteLine("Initialize ServiceConnection");
#if DEBUG
            IsConnected = false;
            Pipe = null;
#else
            var pipeFactory =
                new ChannelFactory<IServicePipe>(
                    new NetNamedPipeBinding
                    {
                        MaxBufferSize = 1048576,
                        MaxBufferPoolSize = 1048576,
                        MaxReceivedMessageSize = 1048576
                    },
                    new EndpointAddress(
                        "net.pipe://localhost/69e001dd06a44ff1b3260a75a6f10381/OrcusUtilities"));
            try
            {
                Pipe = pipeFactory.CreateChannel();
                IsConnected = Pipe.IsAlive();
                Program.WriteLine("Service Connected");
            }
            catch (Exception)
            {
                IsConnected = false;
                Program.WriteLine("Service not connected");
            }
#endif
        }

        public bool IsConnected { get; private set; }
        public IServicePipe Pipe { get; }
        public static ServiceConnection Current => _serviceConnection ?? (_serviceConnection = new ServiceConnection());
    }
}