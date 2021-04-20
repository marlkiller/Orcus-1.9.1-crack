using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class AutostartBuilderProperty : IBuilderProperty
    {
        public StartupMethod AutostartMethod { get; set; } = StartupMethod.Disable;
        public string TaskSchedulerTaskName { get; set; } = "Orcus";
        public bool TaskHighestPrivileges { get; set; } = true;

        public bool RegistryHiddenStart { get; set; } = true;
        public string RegistryKeyName { get; set; } = "Orcus";

        public bool TryAllAutostartMethodsOnFail { get; set; } = true;

        public IBuilderProperty Clone()
        {
            return new AutostartBuilderProperty
            {
                AutostartMethod = AutostartMethod,
                TaskSchedulerTaskName = TaskSchedulerTaskName,
                TaskHighestPrivileges = TaskHighestPrivileges,
                TryAllAutostartMethodsOnFail = TryAllAutostartMethodsOnFail,
                RegistryHiddenStart = RegistryHiddenStart,
                RegistryKeyName = RegistryKeyName
            };
        }
    }

    public enum StartupMethod
    {
        Disable,
        Registry,
        TaskScheduler
    }

    public enum RegistryLocation
    {
        LocalMachine,
        CurrentUser,
        Hidden,
        Situation
    }
}