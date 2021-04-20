using System.Windows.Input;
using System.Windows.Media;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public interface IEntryViewModel
    {
        string Label { get; }
        string Name { get; }
        string SortingName { get; }
        IFileExplorerEntry Value { get; }
        EntryType EntryType { get; }
        ImageSource Icon { get; }
        string Description { get; }
        bool IsDirectory { get; } //important for sorting
        long Size { get; }
        bool IsInRenameMode { get; set; }
        ICommand BeginRenameCommand { get; }
        EntryViewModelCommands Commands { get; }
        ImageSource Thumbnail { get; }
        ImageSource BigThumbnail { get; }
    }

    public static class EntryViewModelExtensions
    {
        public static bool IsFileSystemEntry(this IEntryViewModel entryViewModel)
        {
            return entryViewModel.EntryType == EntryType.File || entryViewModel.EntryType == EntryType.Directory;
        }

        public static bool IsDirectory(this IEntryViewModel entryViewModel)
        {
            return entryViewModel.EntryType == EntryType.Directory;
        }
    }

    public enum EntryType
    {
        File,
        Directory,
        Processing
    }
}