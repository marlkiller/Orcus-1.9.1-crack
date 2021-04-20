using System.Xml.Serialization;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.Core.CrowdControl
{
    [XmlInclude(typeof (CommandPresetWithTarget))]
    public class CommandPreset
    {
        public StaticCommand StaticCommand { get; set; }
        public TransmissionEvent TransmissionEvent { get; set; }
        public ExecutionEvent ExecutionEvent { get; set; }
        public StopEvent StopEvent { get; set; }

        [XmlIgnore]
        public bool IsCommandPreset { get; protected set; } = true;
    }
}