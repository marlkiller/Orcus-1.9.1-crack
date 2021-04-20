using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Orcus.Shared.Commands.TaskManager;

namespace Orcus.Administration.Commands.TaskManager
{
    [Serializable]
    public class AdvancedProcessInfo : ProcessInfo, IDisposable, INotifyPropertyChanged
    {
        [NonSerialized] private List<AdvancedProcessInfo> _childProcesses;
        [NonSerialized] private BitmapImage _icon;
        [NonSerialized] private bool _isExpanded;
        [NonSerialized] private ObservableCollection<AdvancedProcessInfo> _viewChildProcesses;

        public AdvancedProcessInfo()
        {
        }

        public AdvancedProcessInfo(ProcessInfo processInfo)
        {
            Name = processInfo.Name;
            Description = processInfo.Description;
            CompanyName = processInfo.CompanyName;
            WorkingSet = processInfo.WorkingSet;
            PrivateBytes = processInfo.PrivateBytes;
            IconBytes = processInfo.IconBytes;
            Id = processInfo.Id;
            StartTime = processInfo.StartTime;
            CanChangePriorityClass = processInfo.CanChangePriorityClass;
            PriorityClass = processInfo.PriorityClass;
            Status = processInfo.Status;
            ProcessOwner = processInfo.ProcessOwner;
            FileVersion = processInfo.FileVersion;
            ProductVersion = processInfo.ProductVersion;
        }

        public void Dispose()
        {
            _icon?.Dispatcher.Invoke(() => _icon.StreamSource.Dispose());
        }

        public List<AdvancedProcessInfo> ChildProcesses
        {
            get { return _childProcesses; }
            set
            {
                _childProcesses = value;
                ViewChildProcesses = new ObservableCollection<AdvancedProcessInfo>(value);
            }
        }

        public ObservableCollection<AdvancedProcessInfo> ViewChildProcesses
        {
            get { return _viewChildProcesses; }
            set { _viewChildProcesses = value; }
        }

        public BitmapImage Icon
        {
            get
            {
                if (_icon != null)
                    return _icon;

                if (IconBytes == null)
                    return null;

                _icon = new BitmapImage();
                _icon.BeginInit();
                _icon.StreamSource = new MemoryStream(IconBytes);
                _icon.EndInit();
                return _icon;
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

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}