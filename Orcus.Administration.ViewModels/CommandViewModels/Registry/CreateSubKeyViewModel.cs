using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.Registry
{
    public class CreateSubKeyViewModel : PropertyChangedBase
    {
        private RelayCommand _cancelCommand;
        private RelayCommand _createCommand;
        private bool? _dialogResult;

        public CreateSubKeyViewModel(string subKeyLocation)
        {
            SubKeyLocation = subKeyLocation;
        }

        public string SubKeyLocation { get; }
        public string Name { get; set; }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand CreateCommand
        {
            get { return _createCommand ?? (_createCommand = new RelayCommand(parameter => { DialogResult = true; })); }
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