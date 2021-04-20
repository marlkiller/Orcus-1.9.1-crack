using System;

namespace Orcus.Shared.Commands.Keylogger
{
    [Serializable]
    public class NormalText : KeyLogEntry
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}