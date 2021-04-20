using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Orcus.Shared.NetSerializer;
using SettingsDecryption.Old;

namespace SettingsDecryption
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void decryptButton_Click(object sender, EventArgs e)
        {
            encryptedTextTextBox.Text = AES.Decrypt(encryptedTextTextBox.Text, encryptionKeyTextBox.Text);
        }

        private void showIpAddressesButton_Click(object sender, EventArgs e)
        {
            List<IpAddressInfo> list = new Serializer(typeof(List<IpAddressInfo>)).Deserialize<List<IpAddressInfo>>(Convert.FromBase64String(encryptedTextTextBox.Text));
            MessageBox.Show(list.Aggregate(new StringBuilder(), (x, y) => x.AppendLine(y.ToString())).ToString());
        }
    }
}