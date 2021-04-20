using System;

namespace Orcus.Plugins
{
    public class CommandLoadedEventArgs : EventArgs
    {
        public CommandLoadedEventArgs(Type newCommandType)
        {
            NewCommandType = newCommandType;
        }

        public Type NewCommandType { get; }
    }
}