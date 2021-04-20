using System.Windows.Forms;
using Orcus.Server.Core.Plugins;

namespace Orcus.Server.UpdaterView
{
    public partial class SecureDnsUpdater : UserControl, IUpdaterView
    {
        private Core.Plugins.SecureDnsUpdater _secureDnsUpdater;

        public SecureDnsUpdater()
        {
            InitializeComponent();
        }

        public void Initizalize(IUpdatePlugin plugin)
        {
            _secureDnsUpdater = (Core.Plugins.SecureDnsUpdater) plugin;
            UserNameTextBox.Text = _secureDnsUpdater.Settings?.UserName;
            PasswordTextBox.Text = _secureDnsUpdater.Settings?.Password;
            HostTextBox.Text = _secureDnsUpdater.Settings?.HostName;
        }

        public bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(UserNameTextBox.Text))
            {
                MessageBox.Show("Please input a user name or disable the updater.", "Error", MessageBoxButtons.OK,
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

            _secureDnsUpdater.Settings = new Core.Plugins.SecureDnsUpdater.SecureDnsUpdaterSettings
            {
                UserName = UserNameTextBox.Text,
                HostName = HostTextBox.Text,
                Password = PasswordTextBox.Text
            };

            return true;
        }
    }
}