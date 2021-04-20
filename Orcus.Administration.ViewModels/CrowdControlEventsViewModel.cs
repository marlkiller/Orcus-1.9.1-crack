using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.ViewModels.CrowdControl;
using Orcus.Shared.DynamicCommands;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class CrowdControlEventsViewModel : PropertyChangedBase
    {
        private RelayCommand<IList> _copyMessageCommand;
        private EventsFilterType _eventsFilterType;

        public CrowdControlEventsViewModel(DynamicCommandViewModel dynamicCommandViewModel)
        {
            DynamicCommandViewModel = dynamicCommandViewModel;
            EventsCollectionView = new CollectionViewSource {Source = dynamicCommandViewModel.DynamicCommandEvents}.View;
            EventsCollectionView.Filter = EventsFilter;
        }

        public DynamicCommandViewModel DynamicCommandViewModel { get; }
        public ICollectionView EventsCollectionView { get; }

        public EventsFilterType EventsFilterType
        {
            get { return _eventsFilterType; }
            set
            {
                if (SetProperty(value, ref _eventsFilterType))
                    EventsCollectionView.Refresh();
            }
        }

        public RelayCommand<IList> CopyMessageCommand
        {
            get
            {
                return _copyMessageCommand ?? (_copyMessageCommand = new RelayCommand<IList>(parameter =>
                {
                    var selectedEvents = parameter.Cast<DynamicCommandEvent>().ToList();
                    var stringBuilder = new StringBuilder();

                    foreach (var dynamicCommandEvent in selectedEvents)
                    {
                        var message = dynamicCommandEvent.Message;
                        if (string.IsNullOrEmpty(message))
                            message = ActivityToString(dynamicCommandEvent.Status);

                        stringBuilder.AppendLine(message);
                    }

                    Clipboard.SetDataObject(stringBuilder.ToString());
                }));
            }
        }

        public static string ActivityToString(ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.Sent:
                    return (string) Application.Current.Resources["CommandSentToClient"];
                case ActivityType.Succeeded:
                    return (string) Application.Current.Resources["CommandSucceeded"];
                case ActivityType.Failed:
                    return (string) Application.Current.Resources["CommandFailed"];
                case ActivityType.Active:
                    return (string) Application.Current.Resources["ActiveCommandMessage"];
                case ActivityType.Stopped:
                    return (string) Application.Current.Resources["StoppedCommandMessage"];
                default:
                    return null;
            }
        }

        private bool EventsFilter(object o)
        {
            var dynamicCommandEvent = (DynamicCommandEvent) o;
            switch (EventsFilterType)
            {
                case EventsFilterType.Succeeded:
                    return dynamicCommandEvent.Status == ActivityType.Succeeded;
                case EventsFilterType.Failed:
                    return dynamicCommandEvent.Status == ActivityType.Failed;
                default:
                    return true;
            }
        }
    }

    public enum EventsFilterType
    {
        All,
        Succeeded,
        Failed
    }
}