using System;
using System.Linq;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using OrcusPluginStudio.Core.Test.AdministrationVirtualisation;
using ConnectionInfo = OrcusPluginStudio.Core.Test.ClientVirtualisation.ConnectionInfo;

namespace OrcusPluginStudio.Core.Test
{
    public class ClientVirtualizer : IDisposable
    {
        private readonly Command _command;
        private readonly Orcus.Plugins.Command _clientCommand;
        private readonly Sender _administrationSender;
        private readonly ConnectionInfo _clientConnectionInfo;

        public ClientVirtualizer(Command command, Orcus.Plugins.Command clientCommand)
        {
            _command = command;
            _clientCommand = clientCommand;
            _administrationSender = new Sender();
            _administrationSender.SendCommandEvent += _administrationSender_SendCommandEvent;
            ClientController = new ClientController(command, _administrationSender);

            _clientConnectionInfo = new ConnectionInfo();
            _clientConnectionInfo.ResponseData += _clientConnectionInfo_ResponseData;
        }

        private void _clientConnectionInfo_ResponseData(object sender, byte[] e)
        {
            var id = BitConverter.ToInt32(e, 0);
            if (id == _command.Identifier)
                try
                {
                    _command.ResponseReceived(e.Skip(4).ToArray());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private void _administrationSender_SendCommandEvent(object sender, Tuple<uint, byte[]> e)
        {
            if (e.Item1 == _clientCommand.Identifier)
            {
                try
                {
                    _clientCommand.ProcessCommand(e.Item2, _clientConnectionInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public IClientController ClientController { get; }
        public void Dispose()
        {
            _command.Dispose();
            _administrationSender.Dispose();
        }
    }
}