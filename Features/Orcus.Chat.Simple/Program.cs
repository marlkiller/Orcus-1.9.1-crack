using System;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Orcus.Chat.Simple.Core;
using Orcus.Chat.Simple.Utilities;
using Orcus.Commands.TextChat;

namespace Orcus.Chat.Simple
{
    static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var host = new ServiceHost(
                typeof (ChatService), new Uri("net.pipe://localhost/aea9404c4c3542e08537409985536be0"));
            host.AddServiceEndpoint(typeof (IChatClient),
                new NetNamedPipeBinding
                {
                    MaxBufferSize = 1048576,
                    MaxBufferPoolSize = 1048576,
                    MaxReceivedMessageSize = 1048576
                },
                "OrcusChatService");
            try
            {
                host.Open();
            }
            catch (Exception)
            {
                return;
            }

            ChatService chatService = null;
            using (var autoResetEvent = new AutoResetEvent(false))
            {
                ChatService.Initialized += (sender, args) =>
                {
                    chatService = (ChatService) sender;
                    autoResetEvent.Set();
                };
                if (!autoResetEvent.WaitOne(10000))
                    return;
            }

            if (chatService.HideEverything)
                Computer.MinimizeAllScreens();

            chatService.ChatCallback.OpenCallback();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(chatService));
        }
    }
}