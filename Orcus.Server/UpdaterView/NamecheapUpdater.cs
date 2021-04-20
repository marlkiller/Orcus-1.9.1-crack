using System.Windows.Forms;
using Orcus.Server.Core.Plugins;
using System.Diagnostics;

namespace Orcus.Server.UpdaterView
{
    public partial class NamecheapUpdater : UserControl, IUpdaterView
    {
        private Core.Plugins.NamecheapUpdater _namecheapUpdater;

        public NamecheapUpdater()
        {
            InitializeComponent();
        }

        public void Initizalize(IUpdatePlugin plugin)
        {
            _namecheapUpdater = (Core.Plugins.NamecheapUpdater) plugin;
            DomainTextBox.Text = _namecheapUpdater.Settings?.DomainName;
            HostTextBox.Text = _namecheapUpdater.Settings?.Host;
            PasswordTextBox.Text = _namecheapUpdater.Settings?.Password;
        }

        public bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(DomainTextBox.Text))
            {
                MessageBox.Show("Please input a domain name or disable the updater.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(PasswordTextBox.Text))
            {
                MessageBox.Show("Please input a password or disable the updater.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            if (HostTextBox.Text == "@")
                HostTextBox.Text = "";

            _namecheapUpdater.Settings = new Core.Plugins.NamecheapUpdater.NamecheapUpdaterSettings
            {
                DomainName = DomainTextBox.Text,
                Host = HostTextBox.Text,
                Password = PasswordTextBox.Text
            };

            return true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.namecheap.com/support/knowledgebase/article.aspx/29/11/how-do-i-use-a-browser-to-dynamically-update-the-hosts-ip");
        }
    }
}