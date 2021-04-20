using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.ViewModels.CrowdControl;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class CrowdControlExecutingClientsViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private readonly DynamicCommandViewModel _dynamicCommandViewModel;
        private RelayCommand _stopExecutionCommand;
        private string _title;

        public CrowdControlExecutingClientsViewModel(DynamicCommandViewModel dynamicCommandViewModel,
            ConnectionManager connectionManager)
        {
            _dynamicCommandViewModel = dynamicCommandViewModel;
            _connectionManager = connectionManager;
            CollectionView = new CollectionViewSource {Source = dynamicCommandViewModel.ExecutingClients}.View;
            dynamicCommandViewModel.ExecutingClients.CollectionChanged += ExecutingClientsOnCollectionChanged;
            UpdateTitle();
        }

        private void ExecutingClientsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdateTitle();
        }

        public ICollectionView CollectionView { get; }

        public string Title
        {
            get { return _title; }
            set { SetProperty(value, ref _title); }
        }

        private void UpdateTitle()
        {
            Title =
                $"{_dynamicCommandViewModel.CommandType} [ID: {_dynamicCommandViewModel.DynamicCommand.Id}] - {_dynamicCommandViewModel.ExecutingClients?.Count} {Application.Current.Resources["Clients"]}";
        }

        public RelayCommand StopExectionCommand
        {
            get
            {
                return _stopExecutionCommand ?? (_stopExecutionCommand = new RelayCommand(parameter =>
                {
                    var clients = ((IList) parameter).Cast<ClientViewModel>().Select(x => x.Id).ToList();
                    _connectionManager.StopClientActiveCommands(clients, _dynamicCommandViewModel.DynamicCommand);
                }));
            }
        }
    }
}