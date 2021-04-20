using System;
using System.Collections.Generic;

namespace Orcus.StaticCommandManagement
{
    [Serializable]
    public class StopSchedulerSettings
    {
        public StopSchedulerSettings()
        {
            Sessions = new List<SessionCommandInfo>();
            DurationStopEventInfos = new List<DurationStopEventInfo>();
        }

        public List<SessionCommandInfo> Sessions { get; set; }
        public List<DurationStopEventInfo> DurationStopEventInfos { get; set; }
    }

    [Serializable]
    public class SessionCommandInfo
    {
        public int CommandId { get; set; }
        public DateTime StartupTime { get; set; }
    }

    public class DurationStopEventInfo
    {
        public int CommandId { get; set; }
        public DateTime StartTime { get; set; }
    }
}