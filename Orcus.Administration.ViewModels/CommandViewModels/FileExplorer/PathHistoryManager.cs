using System.Collections.Generic;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class PathHistoryManager : PropertyChangedBase
    {
        private readonly Stack<string> _goBackStack;
        private readonly Stack<string> _goForwardStack;

        public PathHistoryManager()
        {
            _goBackStack = new Stack<string>();
            _goForwardStack = new Stack<string>();
        }

        public bool CanGoBack => _goBackStack.Count > 0;
        public bool CanGoForward => _goForwardStack.Count > 0;
        public string CurrentPath { get; private set; }

        public void Navigate(string path)
        {
            if (path == CurrentPath)
                return;

            _goForwardStack.Clear();
            if (CurrentPath != null)
                _goBackStack.Push(CurrentPath);
            CurrentPath = path;

            UpdateProperties();
        }

        public string GoBack()
        {
            var path = _goBackStack.Pop();
            _goForwardStack.Push(CurrentPath);
            CurrentPath = path;
            UpdateProperties();
            return path;
        }

        public string GoForward()
        {
            var path = _goForwardStack.Pop();
            _goBackStack.Push(CurrentPath);
            CurrentPath = path;
            UpdateProperties();
            return path;
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
        }
    }
}