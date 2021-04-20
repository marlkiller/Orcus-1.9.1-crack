using System;

namespace Orcus.Administration.Core.Build
{
    internal class ProgressChangedEventArgs : EventArgs
    {
        public ProgressChangedEventArgs(string progressState)
        {
            ProgressState = progressState;
        }

        public string ProgressState { get; }
    }
}