using System.Windows;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class CrowdControlShortcutSettingsViewModel : PropertyChangedBase
    {
        private RelayCommand _cancelCommand;
        private bool? _dialogResult;
        private RelayCommand _okCommand;

        public CrowdControlShortcutSettingsViewModel(StaticCommand staticCommand)
        {
            StaticCommand = staticCommand;
        }

        public StaticCommand StaticCommand { get; }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { DialogResult = false; }));
            }
        }

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new RelayCommand(parameter =>
                {
                    var validationResult = StaticCommand.ValidateInput();
                    switch (validationResult.ValidationState)
                    {
                        case ValidationState.Error:
                            WindowServiceInterface.Current.ShowMessageBox(validationResult.Message,
                                (string) Application.Current.Resources["Error"],
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        case ValidationState.WarningYesNo:
                            if (WindowServiceInterface.Current.ShowMessageBox(validationResult.Message,
                                (string) Application.Current.Resources["Warning"],
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                                return;

                            break;
                        case ValidationState.Success:
                            break;
                        default:
                            return;
                    }

                    DialogResult = true;
                }));
            }
        }
    }
}