using System;

namespace Orcus.Shared.Commands.Keylogger
{
    [Serializable]
    public class KeyLogPresenter
    {
        public DateTime Timestamp { get; set; }
        public int Id { get; set; }
    }
}