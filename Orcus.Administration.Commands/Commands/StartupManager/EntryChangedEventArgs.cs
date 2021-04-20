using System;
using Orcus.Shared.Commands.StartupManager;

namespace Orcus.Administration.Commands.StartupManager
{
    public class EntryChangedEventArgs : EventArgs
    {
        public EntryChangedEventArgs(string name, AutostartLocation autostartLocation, bool isEnabled)
        {
            Name = name;
            AutostartLocation = autostartLocation;
            IsEnabled = isEnabled;
        }

        public AutostartLocation AutostartLocation { get; }
        public string Name { get; }
        public bool IsEnabled { get; }
    }
}