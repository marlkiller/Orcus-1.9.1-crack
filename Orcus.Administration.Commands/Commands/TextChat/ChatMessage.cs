using System;

namespace Orcus.Administration.Commands.TextChat
{
    public class ChatMessage
    {
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }
        public bool IsFromMe { get; set; }
    }
}