using Orcus.Shared.Commands.SystemRestore;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.SystemRestore
{
    public class CreateSystemRestorePointViewModel : PropertyChangedBase
    {
        private RelayCommand _cancelCommand;
        private RelayCommand _createCommand;
        private string _description;
        private bool? _dialogResult;
        private EventType _eventType;
        private RestoreType _restoreType;

        public EventType EventType
        {
            get { return _eventType; }
            set { SetProperty(value, ref _eventType); }
        }

        public RestoreType RestoreType
        {
            get { return _restoreType; }
            set { SetProperty(value, ref _restoreType); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(value, ref _description); }
        }

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

        public RelayCommand CreateCommand
        {
            get { return _createCommand ?? (_createCommand = new RelayCommand(parameter => { DialogResult = true; })); }
        }
    }
}