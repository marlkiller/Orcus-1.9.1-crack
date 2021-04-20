using System.ComponentModel;
using System.Windows;
using Orcus.Administration.ViewModels;

namespace Orcus.Administration.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow
    {
        public DownloadWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var viewModel = dependencyPropertyChangedEventArgs.NewValue as DownloadViewModel;
            if (viewModel != null)
                viewModel.CloseWindow = Close;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            (DataContext as DownloadViewModel)?.OnWindowClosing(e);
        }
    }
}