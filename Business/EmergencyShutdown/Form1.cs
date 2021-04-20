using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace EmergencyShutdown
{
    public partial class Form1 : Form
    {
        private string _selectedDatabasePath;

        public Form1()
        {
            InitializeComponent();
        }

        public string PrivateKey { get; private set; }

        private void TakeDownButton_Click(object sender, EventArgs e)
        {
            var client = new TcpClient();
            try
            {
                var result = client.BeginConnect(ipTextBox.Text, (int) portNumericUpDown.Value, null, null);
                var success = result.AsyncWaitHandle.WaitOne(3000, false);
                if (!success)
                {
                    MessageBox.Show("Connection failed");
                    return;
                }

                client.EndConnect(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred when trying to connect:\r\n" + ex);
                return;
            }

            using (var sslStream = new SslStream(client.GetStream(), false, (o, certificate, chain, errors) => true))
            {
                try
                {
                    var serverName = Environment.MachineName;
                    sslStream.AuthenticateAsClient(serverName);
                }
                catch (AuthenticationException ex)
                {
                    sslStream.Dispose();
                    client.Close();

                    MessageBox.Show("Authentication failed:\r\n" + ex);
                    return;
                }

                using (var binaryWriter = new BinaryWriter(sslStream))
                using (var binaryReader = new BinaryReader(sslStream))
                {
                    binaryWriter.Write((byte) 0); //Register as client
                    binaryWriter.Write(0); //ClientAcceptor version
                    if (binaryReader.ReadByte() == 2) //OutdatedVersion
                    {
                        MessageBox.Show("Server doesn't support this yet (version < 1.4)");
                        return;
                    }
                    binaryReader.ReadByte(); //GetKey

                    var password = binaryReader.ReadString();

                    using (var rsa = new RSACryptoServiceProvider())
                    {
                        rsa.FromXmlString(PrivateKey);
                        var signedPassword =
                            Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(password), "SHA256"));
                        binaryWriter.Write(signedPassword);
                        var result = binaryReader.ReadInt32();
                        password = BitConverter.ToString(rsa.Encrypt(Encoding.UTF8.GetBytes(password), false));
                        switch (result)
                        {
                            case 0:
                                binaryWriter.Write(
                                    string.Format(richTextBox1.Text.Replace("\n", "\r\n"), password));
                                MessageBox.Show("Server was taken down successfully. Password: " + password);
                                break;
                            case 1:
                                MessageBox.Show("Signature could not be verified");
                                break;
                        }
                    }
                }
            }
        }

        private void selectDatabaseFileButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Orcus Database File|*.sqlite;*.locked|All files|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    databaseFileLabel.Text = Path.GetFileName(ofd.FileName);
                    _selectedDatabasePath = ofd.FileName;
                }
            }
        }

        private void decryptDatabaseButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(encryptionPasswordTextBox.Text))
            {
                MessageBox.Show("No password given");
                return;
            }
            if (string.IsNullOrEmpty(_selectedDatabasePath))
            {
                MessageBox.Show("Please select a database file");
                return;
            }
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(PrivateKey);
                    var unencryptedPassword =
                        rsa.Decrypt(
                            encryptionPasswordTextBox.Text.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray(),
                            false);

                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                        aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                        byte[] salt = {0x10, 0xF5, 0xFE, 0x47, 0x11, 0xDF, 0xAB, 0xA4};
                        // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be exactly the same, including order, on both the encryption and decryption sides.
                        const int iterations = 1042; // Recommendation is >= 1000.
                        using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(unencryptedPassword, salt, iterations))
                        {
                            aes.Key = key.GetBytes(aes.KeySize / 8);
                            aes.IV = key.GetBytes(aes.BlockSize / 8);
                        }

                        aes.Mode = CipherMode.CBC;

                        string destinationFilename;
                        using (var sfd = new SaveFileDialog())
                        {
                            sfd.Filter = "Orcus Database|*.sqlite|All files|*.*";
                            if (sfd.ShowDialog(this) != DialogResult.OK)
                                return;

                            destinationFilename = sfd.FileName;
                        }

                        using (ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV))
                        using (
                            FileStream destination = new FileStream(destinationFilename, FileMode.Create,
                                FileAccess.Write, FileShare.None))
                        using (
                            CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write)
                        )
                        {
                            try
                            {
                                using (
                                    FileStream source = new FileStream(_selectedDatabasePath, FileMode.Open,
                                        FileAccess.Read, FileShare.Read))
                                {
                                    source.CopyTo(cryptoStream);
                                    cryptoStream.FlushFinalBlock();
                                }
                            }
                            catch (CryptographicException exception)
                            {
                                if (exception.Message == "Padding is invalid and cannot be removed.")
                                    throw new ApplicationException(
                                        "Universal Microsoft Cryptographic Exception (Not to be believed!)", exception);
                                throw;
                            }
                        }
                    }
                }

                MessageBox.Show("Database was successfully decrypted!", "Yay", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred when trying to decrypt the database:\r\n" + ex, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var passwordForm = new PasswordForm();
            if (passwordForm.ShowDialog(this) == DialogResult.OK)
                PrivateKey = passwordForm.PrivateKey;
            else
                Close();
        }
    }
}