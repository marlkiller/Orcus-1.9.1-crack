using System.Collections.Generic;
using System.Diagnostics;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class ExecuteFileViewModel : PropertyChangedBase
    {
        private string _arguments;
        private bool _createNoWindow;
        private bool? _dialogResult;
        private RelayCommand _executeCommand;
        private string _verb;

        public ExecuteFileViewModel(FileEntryViewModel fileEntryViewModel)
        {
            Path = fileEntryViewModel.Value.Path;
            Verbs = new List<string> {string.Empty};
            Verbs.AddRange(new ProcessStartInfo(fileEntryViewModel.Value.Path).Verbs);
        }

        public string Path { get; }
        public List<string> Verbs { get; }

        public string Arguments
        {
            get { return _arguments; }
            set { SetProperty(value, ref _arguments); }
        }

        public string Verb
        {
            get { return _verb; }
            set { SetProperty(value, ref _verb); }
        }

        public bool CreateNoWindow
        {
            get { return _createNoWindow; }
            set { SetProperty(value, ref _createNoWindow); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand ExecuteCommand
        {
            get
            {
                return _executeCommand ?? (_executeCommand = new RelayCommand(parameter => { DialogResult = true; }));
            }
        }
    }
}