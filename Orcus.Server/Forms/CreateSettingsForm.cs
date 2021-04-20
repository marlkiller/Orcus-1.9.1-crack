using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Orcus.Server.Utilities;

namespace Orcus.Server.Forms
{
    public partial class CreateSettingsForm : Form
    {
        public CreateSettingsForm()
        {
            InitializeComponent();
        }

        public string Password { get; private set; }
        public string Ip { get; private set; }
        public int Port { get; private set; }
        public string SslCertificatePath { get; private set; }
        public string SslCertificatePassword { get; private set; }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordTextBox.Text))
            {
                MessageBox.Show("Please enter a password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(IpAddressComboBox.Text))
            {
                MessageBox.Show("Please enter an ip address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(SslPathTextBox.Text) && !generateSslCertificateRadioButton.Checked)
            {
                MessageBox.Show("Please select a valid ssl cerfificate or create one", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (generateSslCertificateRadioButton.Checked)
            {
                byte[] certificate;
                var password = GenerateRandomPassword();
                try
                {
                    certificate = Certificate.CreateSelfSignCertificatePfx("CN=OrcusServerCertificate", DateTime.Now.AddYears(-1),
                        DateTime.Now.AddYears(50),
                        password);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                File.WriteAllBytes("certificate.pfx", certificate);
                SslCertificatePassword = password;
                SslCertificatePath = "certificate.pfx";
            }
            else
            {
                try
                {
                    new X509Certificate2(File.ReadAllBytes(SslPathTextBox.Text), SslPasswordTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception while reading the SSL certificate: {ex.Message}", "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                SslCertificatePassword = SslPasswordTextBox.Text;
                SslCertificatePath = SslPathTextBox.Text;
            }

            Password = PasswordTextBox.Text;
            Ip = IpAddressComboBox.Text;
            Port = (int) PortNumericUpDown.Value;

            DialogResult = DialogResult.OK;
        }

        private string GenerateRandomPassword()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            IpAddressComboBox.Items.Add("127.0.0.1");
            IpAddressComboBox.Items.AddRange(
                Dns.GetHostAddresses(Dns.GetHostName())
                    .OrderByDescending(x => x.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => (object) x)
                    .ToArray());
            IpAddressComboBox.SelectedIndex = 0;
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

        private void createSslCertificateRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            createSslCertPanel.Enabled = createSslCertificateRadioButton.Checked;
        }
    }
}