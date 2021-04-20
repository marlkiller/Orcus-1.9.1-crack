using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class RespawnTaskBuilderProperty : IBuilderProperty
    {
        public bool IsEnabled { get; set; }
        public string TaskName { get; set; } = "Orcus Respawner";

        public IBuilderProperty Clone()
        {
            return new RespawnTaskBuilderProperty {IsEnabled = IsEnabled, TaskName = TaskName};
        }
    }
}