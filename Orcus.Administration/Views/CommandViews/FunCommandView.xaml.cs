using System.Diagnostics;
using System.Windows.Navigation;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for FunCommandView.xaml
    /// </summary>
    public partial class FunCommandView
    {
        public FunCommandView()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}