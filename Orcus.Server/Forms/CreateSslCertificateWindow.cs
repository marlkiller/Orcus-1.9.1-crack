using System;
using System.IO;
using System.Windows.Forms;
using Orcus.Server.Utilities;

namespace Orcus.Server.Forms
{
    public partial class CreateSslCertificateWindow : Form
    {
        public CreateSslCertificateWindow()
        {
            InitializeComponent();
        }

        public string Password { get; private set; }
        public string Path { get; private set; }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }

        private void SubjectTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckIfUserInputIsValid();
        }

        private void PasswordTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckIfUserInputIsValid();
        }

        private void CheckIfUserInputIsValid()
        {
            SaveButton.Enabled = !string.IsNullOrEmpty(SubjectTextBox.Text) &&
                                 !string.IsNullOrEmpty(PasswordTextBox.Text);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SubjectTextBox.Text) || string.IsNullOrEmpty(PasswordTextBox.Text))
            {
                MessageBox.Show("Please check your inputs!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var sfd = new SaveFileDialog {Filter = "PFX file|*.pfx"};
            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            var subjectName = SubjectTextBox.Text;
            var password = PasswordTextBox.Text;
            var expired = DateTimePickerEnd.Value;
            var path = sfd.FileName;
            byte[] certificate;

            try
            {
                certificate = Certificate.CreateSelfSignCertificatePfx($"CN={subjectName}", DateTime.Now.AddYears(-1),
                    expired,
                    password);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            File.WriteAllBytes(path, certificate);
            //relative path
            Path = System.IO.Path.GetDirectoryName(path) == System.IO.Path.GetDirectoryName(Application.ExecutablePath)
                ? System.IO.Path.GetFileName(path)
                : path;

            Password = password;
            DialogResult = DialogResult.OK;
        }
    }
}