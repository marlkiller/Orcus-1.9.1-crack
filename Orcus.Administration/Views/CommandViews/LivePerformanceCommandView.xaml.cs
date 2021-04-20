using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Controls;
using Orcus.Administration.ViewModels.CommandViewModels;
using Orcus.Administration.ViewModels.CommandViewModels.LivePerformance;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for LivePerformanceCommandView.xaml
    /// </summary>
    public partial class LivePerformanceCommandView
    {
        private bool _isLoaded;

        public LivePerformanceCommandView()
        {
            InitializeComponent();
            Loaded += LivePerformanceView_Loaded;
            LayoutUpdated += LivePerformanceView_LayoutUpdated;
            MainTabControl.SelectionChanged += MainTabControl_SelectionChanged;
        }

        private void MainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MemoryTabItem.IsSelected)
            {
                MemoryTabItem.LayoutUpdated += MemoryTabItem_LayoutUpdated;
                MainTabControl.SelectionChanged -= MainTabControl_SelectionChanged;
            }
        }

        private void MemoryTabItem_LayoutUpdated(object sender, System.EventArgs e)
        {
            MemoryTabItem.LayoutUpdated -= MemoryTabItem_LayoutUpdated;
            MemoryChart.Margin = new Thickness(0); //To update the chart
        }

        private void LivePerformanceView_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (ActualHeight > 0 || ActualWidth > 0)
            {
                LayoutUpdated -= LivePerformanceView_LayoutUpdated;
                CpuChart.Margin = new Thickness(0); //To update the chart
            }
        }

        private void LivePerformanceView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                ((LivePerformanceViewModel) DataContext).AddEthernetAdapters += OnAddEthernetAdapters;
            }
        }

        private void OnAddEthernetAdapters(object sender, List<EthernetAdapterViewModel> ethernetAdapterViewModels)
        {
            foreach (var ethernetAdapterViewModel in ethernetAdapterViewModels)
                MainTabControl.Items.Add(new TabItem
                {
                    Header = ethernetAdapterViewModel.EthernetAdapter.Description,
                    Content = new EthernetAdapter(ethernetAdapterViewModel)
                });
        }
    }
}