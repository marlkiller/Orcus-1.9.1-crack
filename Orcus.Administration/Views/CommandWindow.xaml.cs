using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Native;
using Orcus.Administration.ViewModels;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow : INotifyPropertyChanged, IFullscreenableWindow
    {
        private const int SYSMENU_TOGGLETOPMOST = 0x1;
        private const int SYSMENU_FULLSCREEN = 0x2;
        private const int MF_CHECKED = 0x8;
        private const int MF_UNCHECKED = 0x0;
        private const int MF_SEPARATOR = 0x800;
        private const int WM_SYSCOMMAND = 0x112;

        private bool _isFullscreen;
        private bool _isTopmost;
        private IntPtr _menuHandle;
        private Popup _settingsPopup;

        public CommandWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        //extra property is important for synchronizing the sys menu item
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set
            {
                if (_isTopmost != value)
                {
                    _isTopmost = value;
                    Topmost = value;
                    NativeMethods.CheckMenuItem(_menuHandle, SYSMENU_TOGGLETOPMOST,
                        (uint) (value ? MF_CHECKED : MF_UNCHECKED));
                    OnPropertyChanged();
                    _settingsPopup.IsOpen = false;
                }
            }
        }

        public bool IsFullscreen
        {
            get { return _isFullscreen; }
            set
            {
                if (_isFullscreen != value)
                {
                    _isFullscreen = value;
                    SetFullscreen(value);
                    NativeMethods.CheckMenuItem(_menuHandle, SYSMENU_FULLSCREEN,
                        (uint) (value ? MF_CHECKED : MF_UNCHECKED));
                    OnPropertyChanged();
                    _settingsPopup.IsOpen = false;
                    FullscreenChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler FullscreenChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            _settingsPopup = (Popup) Resources["WindowSettingsPopup"];
            _settingsPopup.DataContext = this;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape && IsFullscreen)
                IsFullscreen = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = (HwndSource) PresentationSource.FromVisual(this);
            source.AddHook(WndProc);

            var helper = new WindowInteropHelper(this);
            var hSysMenu = NativeMethods.GetSystemMenu(helper.Handle, false);
            NativeMethods.InsertMenu(hSysMenu, 0, MF_UNCHECKED, SYSMENU_TOGGLETOPMOST,
                (string) Application.Current.Resources["Topmost"]);
            NativeMethods.InsertMenu(hSysMenu, 0, MF_UNCHECKED, SYSMENU_FULLSCREEN,
                (string) Application.Current.Resources["Fullscreen"]);
            NativeMethods.InsertMenu(hSysMenu, 1, MF_SEPARATOR, 0, string.Empty);

            _menuHandle = hSysMenu;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND)
            {
                var iwParam = (int) wParam;

                switch (iwParam)
                {
                    case SYSMENU_TOGGLETOPMOST:
                        IsTopmost = !IsTopmost;
                        break;
                    case SYSMENU_FULLSCREEN:
                        IsFullscreen = !IsFullscreen;
                        break;
                }
            }
            return IntPtr.Zero;
        }

        private void SetFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                IgnoreTaskbarOnMaximize = true;
                WindowState = WindowState.Maximized;
                UseNoneWindowStyle = true;
            }
            else
            {
                WindowState = WindowState.Normal;
                UseNoneWindowStyle = false;
                ShowTitleBar = true; // <-- this must be set to true
                IgnoreTaskbarOnMaximize = false;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}