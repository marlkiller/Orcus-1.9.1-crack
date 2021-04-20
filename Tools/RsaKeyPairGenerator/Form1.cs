using System;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace RsaKeyPairGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var rsa = new RSACryptoServiceProvider(1024);
            richTextBox1.Text = rsa.ToXmlString(true);
            richTextBox2.Text = rsa.ToXmlString(false);
        }
    }
}