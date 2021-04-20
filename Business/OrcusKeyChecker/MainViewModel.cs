using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OrcusKeyChecker
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private CurrentState _currentState = CurrentState.Idle;
        private string _licenseKey;
        private KeyCheckTask _keyCheckTask;

        public CurrentState CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LicenseKey
        {
            get { return _licenseKey; }
            set
            {
                if (_licenseKey != value)
                {
                    _licenseKey = value;

                    Guid licenseKey;
                    if (Guid.TryParse(value, out licenseKey))
                    {
                        if (_keyCheckTask?.LicenseKey == licenseKey)
                            return;

                        _keyCheckTask?.Cancel();
                        CurrentState = CurrentState.Checking;
                        CheckKey(licenseKey);
                    }
                    else
                    {
                        _keyCheckTask?.Cancel();
                        CurrentState = CurrentState.Idle;
                    }
                }
            }
        }

        private async void CheckKey(Guid licenseKey)
        {
            _keyCheckTask = new KeyCheckTask(licenseKey);
            var result = await _keyCheckTask.CheckKey();

            switch (result)
            {
                case KeyCheckResult.UnknownResult:
                    break;
                default:
                    CurrentState = (CurrentState) result;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum CurrentState
    {
        ConnectionFailed,
        OperationAborted,
        NotFound,
        Banned,
        Valid,
        UnknownResult,
        Idle,
        Checking
    }
}