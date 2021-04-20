using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerStressTest
{
    public partial class MainForm : Form
    {
        private bool _canceled;
        private List<Client> _clients;
        private bool _isRunning;

        public MainForm()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            IpTextBox.Enabled = false;
            PortNumber.Enabled = false;
            ClientsCount.Enabled = false;

            StopButton.Enabled = true;

            Connection();
        }

        private void Cancel()
        {
            if (_clients != null)
                foreach (var client in _clients)
                {
                    client.Connection?.Dispose();
                }

            StartButton.Enabled = true;
            IpTextBox.Enabled = true;
            PortNumber.Enabled = true;
            ClientsCount.Enabled = true;

            StopButton.Enabled = false;

            StatusLabel.Text = "";
            StatusProgressbar.Value = 0;
        }

        private async void Connection()
        {
            _clients = new List<Client>();

            _canceled = false;
            _isRunning = true;
            for (int i = 0; i < ClientsCount.Value; i++)
            {
                if (_canceled)
                {
                    Cancel();
                    break;
                }
                var client = new Client {AuthenticateAsTestClient = false};
                await Task.Run(() => client.Connect(IpTextBox.Text, (int) PortNumber.Value));

                _clients.Add(client);

                StatusLabel.Text = $"{i + 1} / {ClientsCount.Value}";
                StatusProgressbar.Value = (int) (i/(double) (ClientsCount.Value - 1)*100);
            }
            _isRunning = false;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (_isRunning)
                _canceled = true;
            else
                Cancel();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }
    }
}