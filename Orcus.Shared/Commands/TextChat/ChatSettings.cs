using System;

namespace Orcus.Shared.Commands.TextChat
{
    [Serializable]
    public class ChatSettings
    {
        public bool PreventClose { get; set; }
        public string Title { get; set; }
        public string YourName { get; set; }
        public bool HideEveythingElse { get; set; }
        public bool Topmost { get; set; }
    }
}