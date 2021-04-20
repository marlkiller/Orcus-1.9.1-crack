using System.Windows.Forms;
using Orcus.Server.Core.Plugins;

namespace Orcus.Server.UpdaterView
{
    public partial class NoIpUpdater : UserControl, IUpdaterView
    {
        Core.Plugins.NoIpUpdater _noIpUpdater;

        public NoIpUpdater()
        {
            InitializeComponent();
        }

        public void Initizalize(IUpdatePlugin plugin)
        {
            _noIpUpdater = (Core.Plugins.NoIpUpdater) plugin;
            EmailTextBox.Text = _noIpUpdater.Settings?.EMail;
            PasswordTextBox.Text = _noIpUpdater.Settings?.Password;
            HostTextBox.Text = _noIpUpdater.Settings?.HostName;
        }

        public bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(EmailTextBox.Text))
            {
                MessageBox.Show("Please input an E-Mail address or disable the updater.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(PasswordTextBox.Text))
            {
                MessageBox.Show("Please input a password or disable the updater.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(HostTextBox.Text))
            {
                MessageBox.Show("Please input a host name or disable the updater.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            _noIpUpdater.Settings = new Core.Plugins.NoIpUpdater.NoIpUpdaterSettings
            {
                EMail = EmailTextBox.Text,
                HostName = HostTextBox.Text,
                Password = PasswordTextBox.Text
            };

            return true;
        }
    }
}