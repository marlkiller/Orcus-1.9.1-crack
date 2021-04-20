using Orcus.Shared.Commands.WindowsDrivers;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.WindowsDrivers
{
    public class DriverConfigurationViewModel : PropertyChangedBase
    {
        private string _content;

        public DriverConfigurationViewModel(WindowsDriversFile windowsDriversFile)
        {
            WindowsDriversFile = windowsDriversFile;
        }

        public WindowsDriversFile WindowsDriversFile { get; }

        public string Content
        {
            get { return _content; }
            set { SetProperty(value, ref _content); }
        }
    }
}