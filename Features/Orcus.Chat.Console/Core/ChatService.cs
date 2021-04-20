using System;
using System.ServiceModel;
using Orcus.Commands.TextChat;

namespace Orcus.Chat.Console.Core
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChatClient
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public bool PreventClose { get; set; }
        public bool HideEverything { get; set; }
        public bool Topmost { get; set; }

        public IChatCallback ChatCallback { get; private set; }

        public void Initialize(string title, string name, bool preventClose, bool hideEverything, bool topmost)
        {
            ChatCallback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            Title = title;
            Name = name;
            PreventClose = preventClose;
            HideEverything = hideEverything;
            Topmost = topmost;

            Initialized?.Invoke(this, EventArgs.Empty);
        }

        public void MessageReceived(string chatMessage, DateTime dateTime)
        {
            var meLength = 2 + dateTime.ToLongTimeString().Length + 5;
            var line = $"\r[{dateTime.ToLongTimeString()} {Name}]:";
            var length = (meLength > line.Length ? meLength : line.Length) + 4;

            System.Console.WriteLine(line + new string(' ', length - line.Length) + chatMessage);
        }

        public void Close()
        {
            Environment.Exit(0);
        }

        public static event EventHandler Initialized;
    }
}