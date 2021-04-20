using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Orcus.Shared.Commands.WindowManager;

namespace Orcus.Administration.Commands.WindowManager
{
    [Serializable]
    public class AdvancedWindowInformation : WindowInformation, INotifyPropertyChanged
    {
        [NonSerialized] private List<AdvancedWindowInformation> _childWindows;
        [NonSerialized] private bool _isExpanded;
        [NonSerialized] private ObservableCollection<AdvancedWindowInformation> _viewChildWindows;

        public List<AdvancedWindowInformation> ChildWindows
        {
            get { return _childWindows; }
            set
            {
                _childWindows = value;
                ViewChildWindows = new ObservableCollection<AdvancedWindowInformation>(value);
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<AdvancedWindowInformation> ViewChildWindows
        {
            get { return _viewChildWindows; }
            set { _viewChildWindows = value; }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}