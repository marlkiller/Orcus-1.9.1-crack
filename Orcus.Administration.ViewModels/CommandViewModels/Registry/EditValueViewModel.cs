using System.Windows;
using Orcus.Administration.Core.Utilities;
using Orcus.Shared.Commands.Registry;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.Registry
{
    public class EditValueViewModel : PropertyChangedBase
    {
        private RelayCommand _cancelCommand;
        private bool? _dialogResult;
        private bool _isInCreationMode;
        private string _name;
        private RelayCommand _okCommand;
        private string _title;

        public EditValueViewModel(RegistryValue registryValue)
        {
            RegistryValue = ObjectCopier.Clone(registryValue);
            Name = string.IsNullOrEmpty(registryValue.Key)
                ? (string) Application.Current.Resources["DefaultValue"]
                : registryValue.Key;

            switch (registryValue.ValueKind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    Title = (string) Application.Current.Resources["EditString"];
                    break;
                case RegistryValueKind.Binary:
                    Title = (string) Application.Current.Resources["EditBinary"];
                    break;
                case RegistryValueKind.DWord:
                    Title = (string) Application.Current.Resources["EditDWord"];
                    break;
                case RegistryValueKind.MultiString:
                    Title = (string) Application.Current.Resources["EditMultiString"];
                    break;
                case RegistryValueKind.QWord:
                    Title = (string) Application.Current.Resources["EditQWord"];
                    break;
            }
        }

        public EditValueViewModel(RegistryValueKind registryValueKind)
        {
            switch (registryValueKind)
            {
                case RegistryValueKind.Binary:
                    RegistryValue = new RegistryValueBinary {Value = new byte[0]};
                    Title = (string) Application.Current.Resources["CreateBinaryValue"];
                    break;
                case RegistryValueKind.DWord:
                    RegistryValue = new RegistryValueDWord {Value = 0};
                    Title = (string) Application.Current.Resources["CreateDWord"];
                    break;
                case RegistryValueKind.ExpandString:
                    RegistryValue = new RegistryValueExpandString {Value = ""};
                    Title = (string) Application.Current.Resources["CreateStringValue"];
                    break;
                case RegistryValueKind.MultiString:
                    RegistryValue = new RegistryValueMultiString {Value = new string[0]};
                    Title = (string) Application.Current.Resources["CreateMultiString"];
                    break;
                case RegistryValueKind.QWord:
                    RegistryValue = new RegistryValueQWord {Value = 0};
                    Title = (string) Application.Current.Resources["CreateQWord"];
                    break;
                case RegistryValueKind.String:
                    RegistryValue = new RegistryValueString {Value = ""};
                    Title = (string) Application.Current.Resources["CreateStringValue"];
                    break;
            }

            IsInCreationMode = true;
        }

        public RegistryValue RegistryValue { get; }

        public string Title
        {
            get { return _title; }
            set { SetProperty(value, ref _title); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(value, ref _name); }
        }

        public bool IsInCreationMode
        {
            get { return _isInCreationMode; }
            set { SetProperty(value, ref _isInCreationMode); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new RelayCommand(parameter =>
                {
                    if (IsInCreationMode)
                        RegistryValue.Key = Name;

                    DialogResult = true;
                }));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { DialogResult = false; }));
            }
        }
    }
}