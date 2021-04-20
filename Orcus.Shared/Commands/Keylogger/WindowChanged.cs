using System;

namespace Orcus.Shared.Commands.Keylogger
{
    [Serializable]
    public class WindowChanged : KeyLogEntry
    {
        public WindowChanged(string windowTitle)
        {
            WindowTitle = windowTitle;
            Timestamp = DateTime.Now;
        }

        private WindowChanged()
        {
        }

        public string WindowTitle { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"\r\n================= {WindowTitle} =================\r\n";
        }
    }
}