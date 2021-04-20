using System;

namespace Orcus.Shared.Commands.Keylogger
{
    [Serializable]
    public abstract class KeyLogEntry
    {
        public abstract override string ToString();
    }
}