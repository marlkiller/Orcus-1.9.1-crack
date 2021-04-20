using System;
using System.ServiceModel;
using Orcus.Commands.TextChat;

namespace Orcus.Chat.Simple.Core
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChatClient
    {
        public string Title { get; private set; } = "test";
        public string Name { get; private set; }
        public bool PreventClose { get; private set; }
        public bool HideEverything { get; private set; }
        public bool Topmost { get; private set; }

        public IChatCallback ChatCallback { get; private set; }

        void IChatClient.Initialize(string title, string name, bool preventClose, bool hideEverything, bool topmost)
        {
            ChatCallback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            Title = title;
            Name = name;
            PreventClose = preventClose;
            HideEverything = hideEverything;
            Topmost = topmost;

            Initialized?.Invoke(this, EventArgs.Empty);
        }

        void IChatClient.MessageReceived(string chatMessage, DateTime dateTime)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(chatMessage, dateTime));
        }

        void IChatClient.Close()
        {
            Close?.Invoke(this, EventArgs.Empty);
        }

        public static event EventHandler Initialized;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler Close;
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string content, DateTime timestamp)
        {
            Content = content;
            Timestamp = timestamp;
        }

        public string Content { get; }
        public DateTime Timestamp { get; }
    }
}