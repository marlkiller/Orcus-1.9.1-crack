using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.ExceptionHandling;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ExceptionsViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private string _errorReport;
        private ObservableCollection<ExceptionInfo> _exceptionInfos;
        private bool _isLoading;
        private ExceptionInfo _selectedException;
        private RelayCommand _updateCommand;

        public ExceptionsViewModel(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            UpdateCommand.Execute(null);
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public ObservableCollection<ExceptionInfo> ExceptionInfos
        {
            get { return _exceptionInfos; }
            set { SetProperty(value, ref _exceptionInfos); }
        }

        public string ErrorReport
        {
            get { return _errorReport; }
            set { SetProperty(value, ref _errorReport); }
        }

        public ExceptionInfo SelectedException
        {
            get { return _selectedException; }
            set
            {
                if (SetProperty(value, ref _selectedException) && value != null)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Exception Details");
                    stringBuilder.AppendLine("\tOccurred On: " + value.Timestamp.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.AppendLine("\tException Type: " + value.ErrorType);
                    stringBuilder.AppendLine("\tMessage: " + value.Message);
                    stringBuilder.AppendLine("\tState: " + value.State);
                    stringBuilder.AppendLine("\tClient Version: " + value.ClientVersion);
                    stringBuilder.AppendLine("\tStack Trace: ");
                    foreach (
                        var stackTraceLine in
                            value.StackTrace.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries))
                        stringBuilder.AppendLine("\t" + stackTraceLine);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("Environment Information");
                    stringBuilder.AppendLine("\tTotal Memory: " + value.TotalMemory);
                    stringBuilder.AppendLine("\tAvailable Memory: " + value.AvailableMemory);
                    stringBuilder.AppendLine("\tProcess Memory: " + value.ProcessMemory);
                    stringBuilder.AppendLine("\tOS Name: " + value.OsName);
                    stringBuilder.AppendLine("\tArchitecture: " + (value.Is64BitSystem ? "x64" : "x86"));
                    stringBuilder.AppendLine("\tProcess Type: " + (value.Is64BitProcess ? "x64" : "x86"));
                    stringBuilder.AppendLine("\tAdministrator: " + value.IsAdministrator);
                    stringBuilder.AppendLine("\tService: " + value.IsServiceRunning);
                    stringBuilder.AppendLine("\tRuntime Version: " + value.RuntimeVersion);
                    stringBuilder.AppendLine("\tPath: " + value.ProcessPath);
                    ErrorReport = stringBuilder.ToString();
                }
            }
        }

        public bool Today { get; set; }
        public bool Week { get; set; } = true;
        public bool Month { get; set; }
        public bool Year { get; set; }
        public bool Custom { get; set; }

        public RelayCommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new RelayCommand(async parameter =>
                {
                    DateTime start;
                    DateTime end;

                    if (Today)
                    {
                        start = DateTime.Today;
                        end = DateTime.Now;
                    }
                    else if (Week)
                    {
                        start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                        end = DateTime.Now;
                    }
                    else if (Month)
                    {
                        start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        end = DateTime.Now;
                    }
                    else if (Year)
                    {
                        start = new DateTime(DateTime.Now.Year, 1, 1);
                        end = DateTime.Now;
                    }
                    else if (Custom)
                    {
                        var parameters = (object[]) parameter;
                        if (parameters[0] == null || parameters[1] == null)
                            return;

                        start = (DateTime) parameters[0];
                        end = (DateTime) parameters[1];

                        if (start >= end)
                        {
                            WindowServiceInterface.Current.ShowMessageBox(
                                (string) Application.Current.Resources["StartDateLargerThanEnd"],
                                (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        if (end > DateTime.Now)
                        {
                            WindowServiceInterface.Current.ShowMessageBox(
                                (string) Application.Current.Resources["CantSeeIntoFuture"],
                                (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        end = end.AddHours(23.9);
                    }
                    else
                        return;

                    IsLoading = true;
                    ExceptionInfos =
                        new ObservableCollection<ExceptionInfo>(
                            await Task.Run(() => _connectionManager.GetExceptions(start, end)));
                    IsLoading = false;
                }));
            }
        }
    }
}