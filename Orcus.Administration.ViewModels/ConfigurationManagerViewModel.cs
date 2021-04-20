using System;
using System.Linq;
using System.Windows;
using Orcus.Administration.ViewModels.ClientBuilder;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ConfigurationManagerViewModel : PropertyChangedBase
    {
        private bool? _dialogResult;
        private RelayCommand<BuildConfigurationViewModel> _loadConfigurationCommand;
        private RelayCommand<BuildConfigurationViewModel> _removeBuildConfigurationCommand;
        private RelayCommand<BuildConfigurationViewModel> _renameBuildConfigurationCommand;

        public ConfigurationManagerViewModel(BuildConfigurationManager buildConfigurationManager)
        {
            BuildConfigurationManager = buildConfigurationManager;
        }

        public BuildConfigurationViewModel BuildConfiguration { get; private set; }
        public BuildConfigurationManager BuildConfigurationManager { get; }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand<BuildConfigurationViewModel> LoadConfigurationCommand
        {
            get
            {
                return _loadConfigurationCommand ??
                       (_loadConfigurationCommand = new RelayCommand<BuildConfigurationViewModel>(parameter =>
                       {
                           if (parameter == null)
                               return;

                           BuildConfiguration = parameter;
                           DialogResult = true;
                       }));
            }
        }

        public RelayCommand<BuildConfigurationViewModel> RenameBuildConfigurationCommand
        {
            get
            {
                return _renameBuildConfigurationCommand ??
                       (_renameBuildConfigurationCommand = new RelayCommand<BuildConfigurationViewModel>(parameter =>
                       {
                           if (parameter == null)
                               return;

                           var inputViewModel = new InputTextViewModel(parameter.Name,
                               (string) Application.Current.Resources["Name"],
                               (string) Application.Current.Resources["Change"]);

                           if (
                               WindowServiceInterface.Current.OpenWindowDialog(inputViewModel,
                                   (string) Application.Current.Resources["RenameConfiguration"]) != true)
                               return;

                           var existingItem =
                               BuildConfigurationManager.BuildConfigurations.FirstOrDefault(
                                   x => string.Equals(x.Name, inputViewModel.Text, StringComparison.OrdinalIgnoreCase));
                           if (existingItem != null)
                           {
                               if (
                                   WindowServiceInterface.Current.ShowMessageBox(
                                       (string) Application.Current.Resources["OverwriteConfiguration"],
                                       (string) Application.Current.Resources["Warning"], MessageBoxButton.OKCancel,
                                       MessageBoxImage.Warning) != MessageBoxResult.OK)
                                   return;

                               BuildConfigurationManager.RemoveBuildConfiguration(existingItem);
                           }

                           var buildConfiguration = parameter.BuildConfigurationInfo.BuildConfiguration;
                           buildConfiguration.Name = inputViewModel.Text;
                           BuildConfigurationManager.UpdateBuildConfiguration(parameter, buildConfiguration);
                       }));
            }
        }

        public RelayCommand<BuildConfigurationViewModel> RemoveBuildConfigurationCommand
        {
            get
            {
                return _removeBuildConfigurationCommand ??
                       (_removeBuildConfigurationCommand = new RelayCommand<BuildConfigurationViewModel>(parameter =>
                       {
                           if (parameter == null)
                               return;

                           BuildConfigurationManager.RemoveBuildConfiguration(parameter);
                       }));
            }
        }

        private RelayCommand<BuildConfigurationViewModel> _exportCommand;

        public RelayCommand<BuildConfigurationViewModel> ExportCommand
        {
            get
            {
                return _exportCommand ?? (_exportCommand = new RelayCommand<BuildConfigurationViewModel>(parameter =>
                {
                    if (parameter == null)
                        return;


                }));
            }
        }
    }
}