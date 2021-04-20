using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core.Annotations;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for VersionControl.xaml
    /// </summary>
    public partial class VersionControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedVersionProperty = DependencyProperty.Register(
            "SelectedVersion", typeof (string), typeof (VersionControl),
            new FrameworkPropertyMetadata(OnPropertyChanged)
            {
                DefaultValue = "1.0.0.0",
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        private int _build;
        private int _major;
        private int _minor;
        private int _revision;
        private bool _isUpdating;

        public VersionControl()
        {
            InitializeComponent();
            Major = 1;
            Minor = 0;
            Revision = 0;
            Build = 0;
        }

        public string SelectedVersion
        {
            get { return (string) GetValue(SelectedVersionProperty); }
            set { SetValue(SelectedVersionProperty, value); }
        }

        public int Major
        {
            get { return _major; }
            set
            {
                if (value != _major)
                {
                    _major = value;
                    RefreshVersion();
                    OnPropertyChanged();
                }
            }
        }

        public int Minor
        {
            get { return _minor; }
            set
            {
                if (value != _minor)
                {
                    _minor = value;
                    RefreshVersion();
                    OnPropertyChanged();
                }
            }
        }

        public int Revision
        {
            get { return _revision; }
            set
            {
                if (value != _revision)
                {
                    _revision = value;
                    RefreshVersion();
                    OnPropertyChanged();
                }
            }
        }

        public int Build
        {
            get { return _build; }
            set
            {
                if (value != _build)
                {
                    _build = value;
                    RefreshVersion();
                    OnPropertyChanged();
                }
            }
        }

        private static void OnPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (string.IsNullOrWhiteSpace((string) dependencyPropertyChangedEventArgs.NewValue))
                return;

            var match = Regex.Match((string) dependencyPropertyChangedEventArgs.NewValue,
                @"^(?<major>([0-9]+))\.(?<minor>([0-9]+))\.(?<revision>([0-9]+))\.(?<build>([0-9]+))$");
            if (match.Success)
            {
                var versionControl = (VersionControl) dependencyObject;
                versionControl._isUpdating = true;
                versionControl.Major = int.Parse(match.Groups["major"].Value);
                versionControl.Minor = int.Parse(match.Groups["minor"].Value);
                versionControl.Revision = int.Parse(match.Groups["revision"].Value);
                versionControl.Build = int.Parse(match.Groups["build"].Value);
                versionControl._isUpdating = false;
            }
        }

        private void RefreshVersion()
        {
            if (!_isUpdating)
                SelectedVersion = $"{Major}.{Minor}.{Revision}.{Minor}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}