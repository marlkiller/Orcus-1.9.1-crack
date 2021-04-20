using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Orcus.Shared.Core;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for NetFrameworkOperatingSystemAvailabilityControl.xaml
    /// </summary>
    public partial class NetFrameworkOperatingSystemAvailabilityControl
    {
        public static readonly DependencyProperty FrameworkVersionProperty = DependencyProperty.Register(
            "FrameworkVersion", typeof (FrameworkVersion), typeof (NetFrameworkOperatingSystemAvailabilityControl),
            new PropertyMetadata((FrameworkVersion) (-1), PropertyChangedCallback));

        private readonly SolidColorBrush _downloadSolidColorBrush;
        private readonly SolidColorBrush _includedSolidColorBrush;
        private readonly SolidColorBrush _mustEnableSolidColorBrush;
        private readonly SolidColorBrush _notAvailableSolidColorBrush;
        private readonly SolidColorBrush _updatedToSolidColorBrush;

        public NetFrameworkOperatingSystemAvailabilityControl()
        {
            InitializeComponent();
            _downloadSolidColorBrush = new SolidColorBrush(new Color {R = 243, G = 156, B = 18, A = 255});
            _includedSolidColorBrush = new SolidColorBrush(new Color {R = 46, G = 204, B = 113, A = 255});
            _mustEnableSolidColorBrush = _downloadSolidColorBrush;
            _notAvailableSolidColorBrush = new SolidColorBrush(new Color {R = 231, G = 76, B = 60, A = 255});
            _updatedToSolidColorBrush = _includedSolidColorBrush;

            _downloadSolidColorBrush.Freeze();
            _includedSolidColorBrush.Freeze();
            _mustEnableSolidColorBrush.Freeze();
            _notAvailableSolidColorBrush.Freeze();
            _updatedToSolidColorBrush.Freeze();
        }

        public FrameworkVersion FrameworkVersion
        {
            get { return (FrameworkVersion) GetValue(FrameworkVersionProperty); }
            set { SetValue(FrameworkVersionProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as NetFrameworkOperatingSystemAvailabilityControl;
            if (control == null)
                throw new ArgumentException();

            control.GenerateView((FrameworkVersion) dependencyPropertyChangedEventArgs.NewValue);
        }

        private void GenerateView(FrameworkVersion frameworkVersion)
        {
            //Source: http://johnhaller.com/useful-stuff/dot-net-portable-apps
            switch (frameworkVersion)
            {
                case FrameworkVersion.NET35:
                    SetOsValue(WinXpGrid, WinXpText, Availability.Download);
                    SetOsValue(WinVistaGrid, WinVistaText, Availability.UpdatedTo);
                    SetOsValue(Win7Grid, Win7Text, Availability.Included);
                    SetOsValue(Win8Grid, Win8Text, Availability.MustEnable);
                    SetOsValue(Win10Grid, Win10Text, Availability.MustEnable);
                    break;
                case FrameworkVersion.NET40:
                    SetOsValue(WinXpGrid, WinXpText, Availability.Download);
                    SetOsValue(WinVistaGrid, WinVistaText, Availability.Download);
                    SetOsValue(Win7Grid, Win7Text, Availability.Download);
                    SetOsValue(Win8Grid, Win8Text, Availability.Included);
                    SetOsValue(Win10Grid, Win10Text, Availability.MustEnable);
                    break;
                case FrameworkVersion.NET45:
                    SetOsValue(WinXpGrid, WinXpText, Availability.NotAvailable);
                    SetOsValue(WinVistaGrid, WinVistaText, Availability.Download);
                    SetOsValue(Win7Grid, Win7Text, Availability.Download);
                    SetOsValue(Win8Grid, Win8Text, Availability.Included);
                    SetOsValue(Win10Grid, Win10Text, Availability.Included);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(frameworkVersion), frameworkVersion, null);
            }
        }

        private void SetOsValue(Grid grid, TextBlock textBlock, Availability availability)
        {
            switch (availability)
            {
                case Availability.Download:
                    textBlock.Text = (string) Application.Current.Resources["Download"];
                    grid.Background = _downloadSolidColorBrush;
                    break;
                case Availability.Included:
                    textBlock.Text = (string) Application.Current.Resources["Included"];
                    grid.Background = _includedSolidColorBrush;
                    break;
                case Availability.MustEnable:
                    textBlock.Text = (string) Application.Current.Resources["MustEnable"];
                    grid.Background = _mustEnableSolidColorBrush;
                    break;
                case Availability.NotAvailable:
                    textBlock.Text = (string) Application.Current.Resources["NotAvailable"];
                    grid.Background = _notAvailableSolidColorBrush;
                    textBlock.Foreground = Brushes.White;
                    break;
                case Availability.UpdatedTo:
                    textBlock.Text = (string) Application.Current.Resources["UpdatedTo"];
                    grid.Background = _updatedToSolidColorBrush;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(availability), availability, null);
            }
        }

        private enum Availability
        {
            Download,
            Included,
            MustEnable,
            NotAvailable,
            UpdatedTo
        }
    }
}