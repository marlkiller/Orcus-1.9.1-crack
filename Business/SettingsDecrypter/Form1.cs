using System;
using System.Windows.Forms;
using Orcus.Shared.Encryption;

namespace SettingsDecrypter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void decryptButton_Click(object sender, EventArgs e)
        {
            var decrypted = AES.Decrypt(settingsRichTextBox.Text, signatureTextBox.Text);
            settingsRichTextBox.Text = decrypted;
        }
    }
}