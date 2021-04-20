using System;
using System.ServiceModel;
using System.ServiceProcess;

namespace Orcus.Service
{
    public partial class WindowsService : ServiceBase
    {
        private ServiceHost _host;

        public WindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        protected override void OnStop()
        {
            using (_host)
                _host.Close();
        }

        public void Start()
        {
            _host = new ServiceHost(
                typeof (ServicePipe), new Uri("net.pipe://localhost/69e001dd06a44ff1b3260a75a6f10381"));
            _host.AddServiceEndpoint(typeof (IServicePipe),
                new NetNamedPipeBinding
                {
                    MaxBufferSize = 1048576,
                    MaxBufferPoolSize = 1048576,
                    MaxReceivedMessageSize = 1048576
                },
                "OrcusUtilities");
            _host.Open();
        }
    }
}