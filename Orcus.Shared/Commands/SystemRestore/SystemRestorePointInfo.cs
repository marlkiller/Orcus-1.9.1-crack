using System;

namespace Orcus.Shared.Commands.SystemRestore
{
    [Serializable]
    public class SystemRestorePointInfo
    {
        public uint SequenceNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public RestoreType RestorePointType { get; set; }
        public EventType EventType { get; set; }
    }

    public enum RestoreType : uint
    {
        ApplicationInstall = 0, // Installing a new application
        ApplicationUninstall = 1, // An application has been uninstalled
        ModifySettings = 12, // An application has had features added or removed
        CancelledOperation = 13, // An application needs to delete the restore point it created
        Restore = 6, // System Restore
        Checkpoint = 7, // Checkpoint
        DeviceDriverInstall = 10, // Device driver has been installed
        FirstRun = 11, // Program used for 1st time
        BackupRecovery = 14 // Restoring a backup
    }

    public enum EventType : uint
    {
        BEGIN_NESTED_SYSTEM_CHANGE = 102,
        BEGIN_SYSTEM_CHANGE = 100,
        END_NESTED_SYSTEM_CHANGE = 103,
        END_SYSTEM_CHANGE = 101
    }
}