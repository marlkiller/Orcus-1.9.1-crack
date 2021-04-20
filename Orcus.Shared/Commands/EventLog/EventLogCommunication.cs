namespace Orcus.Shared.Commands.EventLog
{
    public enum EventLogCommunication : byte
    {
        RequestEventLog,
        ResponseEventLogEntries,
        ResponseNoAdministratorRights
    }
}