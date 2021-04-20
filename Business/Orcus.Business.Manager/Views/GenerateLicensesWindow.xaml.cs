using System.Collections.Generic;
using Orcus.Business.Manager.Core.Data;
using Orcus.Business.Manager.ViewModels;

namespace Orcus.Business.Manager.Views
{
    /// <summary>
    ///     Interaction logic for GenerateLicensesWindow.xaml
    /// </summary>
    public partial class GenerateLicensesWindow
    {
        public GenerateLicensesWindow()
        {
            InitializeComponent();
            ((GenerateLicensesViewModel) DataContext).LicensesAdded += GenerateLicensesWindow_LicensesAdded;
            Licenses = new List<License>();
        }

        public List<License> Licenses { get; }

        private void GenerateLicensesWindow_LicensesAdded(object sender, List<License> e)
        {
            Licenses.AddRange(e);
        }
    }
}