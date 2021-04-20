using Orcus.Business.Manager.Core.Data;
using Orcus.Business.Manager.ViewModels;

namespace Orcus.Business.Manager.Views
{
    /// <summary>
    ///     Interaction logic for LicenseInfoWindow.xaml
    /// </summary>
    public partial class LicenseInfoWindow
    {
        public LicenseInfoWindow(License license, DatabaseInfo databaseInfo)
        {
            InitializeComponent();
            var viewModel = new LicenseInformationViewModel(license, databaseInfo);
            DataContext = viewModel;
            viewModel.RefreshLicenses += ViewModel_RefreshLicenses;
        }

        public bool RefreshLicenses { get; private set; }

        private void ViewModel_RefreshLicenses(object sender, System.EventArgs e)
        {
            RefreshLicenses = true;
        }
    }
}