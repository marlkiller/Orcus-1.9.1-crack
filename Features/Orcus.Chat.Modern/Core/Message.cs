using System;

namespace Orcus.Chat.Modern.Core
{
    public class Message
    {
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromMe { get; set; }
    }
}