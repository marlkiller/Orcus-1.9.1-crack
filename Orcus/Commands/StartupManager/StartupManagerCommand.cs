using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using Orcus.Plugins;
using Orcus.Shared.Commands.StartupManager;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.StartupManager
{
    public class StartupManagerCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            byte[] data;
            switch ((StartupManagerCommunication) parameter[0])
            {
                case StartupManagerCommunication.GetAutostartEntries:
                    ResponseBytes((byte) StartupManagerCommunication.ResponseAutostartEntries,
                        new Serializer(typeof (List<AutostartProgramInfo>)).Serialize(
                            AutostartManager.GetAllAutostartPrograms()),
                        connectionInfo);
                    break;
                case StartupManagerCommunication.EnableAutostartEntry:
                    try
                    {
                        AutostartManager.ChangeAutostartEntry((AutostartLocation) parameter[1],
                            Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2), true);
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is SecurityException) && !(ex is UnauthorizedAccessException))
                            throw;
                        data = new byte[parameter.Length];
                        data[0] = (byte) StartupManagerCommunication.ResponseAutostartChangingFailed;
                        Array.Copy(parameter, 1, data, 1, parameter.Length - 1);
                        connectionInfo.CommandResponse(this, data);
                        return;
                    }
                    data = new byte[parameter.Length];
                    data[0] = (byte) StartupManagerCommunication.ResponseAutostartEntryEnabled;
                    Array.Copy(parameter, 1, data, 1, parameter.Length - 1);
                    connectionInfo.CommandResponse(this, data);
                    break;
                case StartupManagerCommunication.DisableAutostartEntry:
                    try
                    {
                        AutostartManager.ChangeAutostartEntry((AutostartLocation) parameter[1],
                            Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2), false);
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is SecurityException) && !(ex is UnauthorizedAccessException))
                            throw;

                        data = new byte[parameter.Length];
                        data[0] = (byte) StartupManagerCommunication.ResponseAutostartChangingFailed;
                        Array.Copy(parameter, 1, data, 1, parameter.Length - 1);
                        connectionInfo.CommandResponse(this, data);
                        return;
                    }
                    data = new byte[parameter.Length];
                    data[0] = (byte) StartupManagerCommunication.ResponseAutostartEntryDisabled;
                    Array.Copy(parameter, 1, data, 1, parameter.Length - 1);
                    connectionInfo.CommandResponse(this, data);
                    break;
                case StartupManagerCommunication.RemoveAutostartEntry:
                    AutostartManager.RemoveAutostartEntry((AutostartLocation) parameter[2],
                        Encoding.UTF8.GetString(parameter, 3, parameter.Length - 3), parameter[1] == 1);
                    data = new byte[parameter.Length];
                    data[0] = (byte) StartupManagerCommunication.ResponseAutostartEntryRemoved;
                    Array.Copy(parameter, 1, data, 1, parameter.Length - 1);
                    connectionInfo.CommandResponse(this, data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 25;
        }
    }
}