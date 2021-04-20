using Orcus.Administration.Resources;

namespace Orcus.Administration.Views.Licensing.Pages
{
    /// <summary>
    ///     Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2
    {
        public Page2()
        {
            InitializeComponent();
            EulaTextBox.Text = Licenses.OrcusEULA;
        }
    }
}