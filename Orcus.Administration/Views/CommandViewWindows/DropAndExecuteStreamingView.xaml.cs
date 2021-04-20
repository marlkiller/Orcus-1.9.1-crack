using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Orcus.Administration.ViewModels.CommandViewModels;

namespace Orcus.Administration.Views.CommandViewWindows
{
    /// <summary>
    ///     Interaction logic for DropAndExecuteStreamingView.xaml
    /// </summary>
    public partial class DropAndExecuteStreamingView
    {
        public DropAndExecuteStreamingView()
        {
            InitializeComponent();
            PreviewKeyDown += ApplicationImageOnPreviewKeyDown;
            PreviewKeyUp += ApplicationImageOnPreviewKeyUp;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ApplicationImage.Focus();
        }

        private void ApplicationImageOnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            ((DropAndExecuteViewModel)DataContext).ApplicationImageOnKeyUp(e);
        }

        private void ApplicationImageOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ((DropAndExecuteViewModel) DataContext).ApplicationImageOnKeyDown(e);
        }

        private void ApplicationImageOnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var applicationImage = (Image) sender;
            ((DropAndExecuteViewModel) DataContext).ApplicationImageOnMouseDown(e, applicationImage.RenderSize,
                e.GetPosition(applicationImage));
        }

        private void ApplicationImageOnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var applicationImage = (Image) sender;
            ((DropAndExecuteViewModel)DataContext).ApplicationImageOnMouseUp(e, applicationImage.RenderSize,
                e.GetPosition(applicationImage));
        }

        private void ApplicationImageOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var applicationImage = (Image) sender;
            ((DropAndExecuteViewModel) DataContext).ApplicationImageOnMouseWheel(e, applicationImage.RenderSize,
                e.GetPosition(applicationImage));
        }
    }
}