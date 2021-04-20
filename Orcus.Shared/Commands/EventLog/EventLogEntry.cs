using System;

namespace Orcus.Shared.Commands.EventLog
{
    [Serializable]
    public class EventLogEntry
    {
        public DateTime Timestamp { get; set; }
        public EventLogEntryType EntryType { get; set; }
        public string Source { get; set; }
        public int EventId { get; set; }
        public string Message { get; set; }
    }
}