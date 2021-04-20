using System.Windows.Input;
using Orcus.Administration.ViewModels.CommandViewModels;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for RemoteDesktopCommandView.xaml
    /// </summary>
    public partial class RemoteDesktopCommandView
    {
        public RemoteDesktopCommandView()
        {
            InitializeComponent();

            PreviewKeyDown += DesktopImageOnPreviewKeyDown;
            PreviewKeyUp += DesktopImageOnPreviewKeyUp;
        }

        /*
        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _keyEventControl.PreviewKeyDown -= DesktopImageOnPreviewKeyDown;
            _keyEventControl.PreviewKeyUp -= DesktopImageOnPreviewKeyUp;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _keyEventControl = ((RemoteDesktopViewModel) DataContext).WindowService.IsExternalWindow
                ? (Control) this
                : Application.Current.MainWindow;

            _keyEventControl.PreviewKeyDown += DesktopImageOnPreviewKeyDown;
            _keyEventControl.PreviewKeyUp += DesktopImageOnPreviewKeyUp;
        }
        */

        private void DesktopImageOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DesktopImage.Focus();
            ((RemoteDesktopViewModel) DataContext).DesktopImageOnMouseDown(e, DesktopImage.RenderSize,
                e.GetPosition(DesktopImage));
        }

        private void DesktopImageOnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ((RemoteDesktopViewModel) DataContext).DesktopImageOnMouseWheel(e, DesktopImage.RenderSize,
                e.GetPosition(DesktopImage));
        }

        private void DesktopImageOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((RemoteDesktopViewModel) DataContext).DesktopImageOnMouseUp(e, DesktopImage.RenderSize,
                e.GetPosition(DesktopImage));
        }

        private void DesktopImageOnMouseMove(object sender, MouseEventArgs e)
        {
            ((RemoteDesktopViewModel) DataContext).DesktopImageOnMouseMove(DesktopImage.RenderSize,
                e.GetPosition(DesktopImage));
        }

        private void DesktopImageOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (((RemoteDesktopViewModel) DataContext).IsStreaming)
            {
                ((RemoteDesktopViewModel) DataContext).DesktopImageOnKeyDown(e);
                e.Handled = true;
            }
        }

        private void DesktopImageOnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (((RemoteDesktopViewModel) DataContext).IsStreaming)
            {
                ((RemoteDesktopViewModel) DataContext).DesktopImageOnKeyUp(e);
                e.Handled = true;
            }
        }
    }
}