using System.Windows.Controls;
using System.Windows.Input;

namespace Orcus.Business.Manager.Views
{
    /// <summary>
    ///     Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public string Password { get; private set; }

        private void PasswordBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            Password = ((PasswordBox) sender).Password;
            if (string.IsNullOrWhiteSpace(Password))
                return;

            DialogResult = true;
        }
    }
}