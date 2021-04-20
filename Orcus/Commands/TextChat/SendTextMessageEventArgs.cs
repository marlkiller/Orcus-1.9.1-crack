using System;

namespace Orcus.Commands.TextChat
{
    public class SendTextMessageEventArgs : EventArgs
    {
        public SendTextMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}