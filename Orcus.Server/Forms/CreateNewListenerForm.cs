using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Orcus.Server.Forms
{
    public partial class CreateNewListenerForm : Form
    {
        public CreateNewListenerForm()
        {
            InitializeComponent();
        }

        public string IpAddress { get; set; }
        public int Port { get; set; }

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

        private void AbortButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(IpAddressComboBox.Text))
            {
                MessageBox.Show("Please enter an ip address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            IpAddress = IpAddressComboBox.Text;
            Port = (int) PortNumericUpDown.Value;
            DialogResult = DialogResult.OK;
        }
    }
}