using System.Windows;
using Orcus.Shared.Commands.ComputerInformation;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for ComputerInformationViewerControl.xaml
    /// </summary>
    public partial class ComputerInformationViewerControl
    {
        public static readonly DependencyProperty ComputerInformationProperty = DependencyProperty.Register(
            "ComputerInformation", typeof (ComputerInformation), typeof (ComputerInformationViewerControl),
            new PropertyMetadata(default(ComputerInformation)));

        public ComputerInformationViewerControl()
        {
            InitializeComponent();
        }

        public ComputerInformation ComputerInformation
        {
            get { return (ComputerInformation) GetValue(ComputerInformationProperty); }
            set { SetValue(ComputerInformationProperty, value); }
        }
    }
}