using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.TaskManager
{
    [Serializable]
    public class ProcessListChangelog
    {
        public List<int> ClosedProcesses { get; set; }
        public List<ProcessInfo> NewProcesses { get; set; }
    }
}