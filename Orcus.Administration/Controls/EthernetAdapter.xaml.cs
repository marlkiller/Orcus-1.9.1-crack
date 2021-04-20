using System.Windows;
using Orcus.Administration.ViewModels.CommandViewModels.LivePerformance;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for EthernetAdapter.xaml
    /// </summary>
    public partial class EthernetAdapter
    {
        public EthernetAdapter(EthernetAdapterViewModel ethernetAdapterViewModel)
        {
            InitializeComponent();
            DataContext = ethernetAdapterViewModel;
            ethernetAdapterViewModel.MaximumChanged += EthernetAdapterModel_MaximumChanged;
            LayoutUpdated += EthernetAdapter_LayoutUpdated;
        }

        private void EthernetAdapter_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (ActualHeight > 0 || ActualWidth > 0)
            {
                LayoutUpdated -= EthernetAdapter_LayoutUpdated;
                SparrowChart.Margin = new Thickness(0); //To update the chart
            }
        }

        private void EthernetAdapterModel_MaximumChanged(object sender, int e)
        {
            GraphLinearYAxis.MaxValue = e;
            GraphLinearYAxis.Interval = e/10;
        }
    }
}