using System.Windows;

namespace Orcus.Business.Manager.Views
{
    /// <summary>
    ///     Interaction logic for ChangeCommentWindow.xaml
    /// </summary>
    public partial class ChangeCommentWindow
    {
        public ChangeCommentWindow()
        {
            InitializeComponent();
        }

        public string Comment { get; private set; }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Comment = CommentTextBox.Text;
            DialogResult = true;
        }
    }
}