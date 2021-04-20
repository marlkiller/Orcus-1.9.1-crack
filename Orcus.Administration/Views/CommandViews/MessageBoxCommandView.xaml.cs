using System.Windows.Input;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for MessageBoxCommandView.xaml
    /// </summary>
    public partial class MessageBoxCommandView
    {
        public MessageBoxCommandView()
        {
            InitializeComponent();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.H && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                MessageTextBox.Text =
                    "Hi, I am a <your country> virus but because of poor technology in my country unfortunately I am not able to harm your computer. Please be so kind to delete one of your important files yourself and then forward me to other users. Many thanks for your cooperation! Best regards, <your country> virus!";
        }
    }
}