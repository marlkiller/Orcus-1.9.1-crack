using System;
using System.Windows;
using Exceptionless;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ExceptionDialogViewModel : PropertyChangedBase
    {
        private RelayCommand _closeCommand;
        private bool _isLoading;
        private RelayCommand _sendExceptionCommand;
        private bool _showDetails;
        private RelayCommand _toggleDetailsCommand;

        public ExceptionDialogViewModel(Exception exception)
        {
            Exception = exception;
            ExceptionType = exception.GetType().Name;
        }

        public Exception Exception { get; }
        public string ExceptionType { get; }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public bool ShowDetails
        {
            get { return _showDetails; }
            set { SetProperty(value, ref _showDetails); }
        }

        public RelayCommand ToggleDetailsCommand
        {
            get
            {
                return _toggleDetailsCommand ??
                       (_toggleDetailsCommand = new RelayCommand(parameter => { ShowDetails = !_showDetails; }));
            }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand ??
                       (_closeCommand = new RelayCommand(parameter => { Close?.Invoke(this, EventArgs.Empty); }));
            }
        }

        public RelayCommand SendExeceptionCommand
        {
            get
            {
                return _sendExceptionCommand ?? (_sendExceptionCommand = new RelayCommand(async parameter =>
                {
                    IsLoading = true;
                    var message = parameter as string;
                    var builder = Exception.ToExceptionless();
                    if (!string.IsNullOrEmpty(message))
                        builder.SetMessage(message);

                    builder.Submit();
                    await ExceptionlessClient.Default.ProcessQueueAsync();
                    Application.Current.Shutdown();
                }));
            }
        }

        public event EventHandler Close;
    }
}