using System;

namespace Orcus.Shared.Commands.MessageBox
{
    [Serializable]
    public class MessageBoxInformation
    {
        public string Text { get; set; }
        public string Title { get; set; }
        public MessageBoxButtons MessageBoxButtons { get; set; }
        public SystemIcon Icon { get; set; }
    }
}