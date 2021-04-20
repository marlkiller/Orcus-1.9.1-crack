using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using NLog;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.FileExplorer.Utilities;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration
{
    public class WpfWindowServiceInterface : IWindowServiceInterface
    {
        private readonly Stack<Window> _windowStack;
        private readonly Dictionary<Type, IWindowViewModel> _windowViewModels;
        private readonly Dictionary<Type, IViewViewModel> _viewViewModels;
        private readonly Dictionary<object, Window> _allWindows;

        public WpfWindowServiceInterface()
        {
            _windowViewModels = new Dictionary<Type, IWindowViewModel>();
            _viewViewModels = new Dictionary<Type, IViewViewModel>();
            _windowStack = new Stack<Window>();
            _allWindows = new Dictionary<object, Window>();
        }

        public void RegisterMainWindow()
        {
            var window = Application.Current.MainWindow;
            _windowStack.Push(window);
            window.Closed += (sender, args) =>
            {
                if (_windowStack.Peek() == window)
                    _windowStack.Pop();
                else
                    LogManager.GetLogger("WpfWindowServiceInterface")
                        .Error("MainWindow was closed and is not on top of the window stack, this may cause problems");
            };
        }

        public void RegisterWindow<TWindow, TViewModel>() where TWindow : Window, new()
        {
            _windowViewModels.Add(typeof (TViewModel), new WindowViewModel<TWindow>());
        }

        public void RegisterView<TView, TViewModel>() where TView : FrameworkElement, new()
        {
            _viewViewModels.Add(typeof (TViewModel), new ViewViewModel<TView>());
        }

        public FrameworkElement GetView<TViewModel>(TViewModel viewModel)
        {
            var view = _viewViewModels[typeof (TViewModel)].GetView();
            view.DataContext = viewModel;
            return view;
        }

        public IWindow GetCurrentWindow()
        {
            return new WpfWindow(_windowStack.Peek());
        }

        public Lazy<IWindow> OpenWindow<TViewModel>(TViewModel viewModel)
        {
            var windowViewModel = _windowViewModels[typeof (TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.Show();
            AddWindow(viewModel, window);

            return new Lazy<IWindow>(() => new WpfWindow(window));
        }

        public Lazy<IWindow> OpenWindow<TViewModel>(TViewModel viewModel, string title)
        {
            var windowViewModel = _windowViewModels[typeof (TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.Title = title;
            window.Show();
            AddWindow(viewModel, window);

            return new Lazy<IWindow>(() => new WpfWindow(window));
        }

        public Lazy<IWindow> OpenWindowCentered<TViewModel>(TViewModel viewModel)
        {
            var windowViewModel = _windowViewModels[typeof (TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.CenterOnWindow(_windowStack.Peek());
            window.Show();
            AddWindow(viewModel, window);

            return new Lazy<IWindow>(() => new WpfWindow(window));
        }

        public Lazy<IWindow> OpenWindowCentered<TViewModel>(IWindowService windowService, TViewModel viewModel)
        {
            var windowViewModel = _windowViewModels[typeof(TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            windowService.OpenWindowCentered(window);
            AddWindow(viewModel, window);

            return new Lazy<IWindow>(() => new WpfWindow(window));
        }

        public Lazy<IWindow> OpenWindowCentered<TViewModel>(TViewModel viewModel, string title)
        {
            var windowViewModel = _windowViewModels[typeof (TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.Title = title;
            window.CenterOnWindow(_windowStack.Peek());
            window.Show();
            AddWindow(viewModel, window);

            return new Lazy<IWindow>(() => new WpfWindow(window));
        }

        public Lazy<IWindow> OpenWindowCentered<TViewModel>(IWindowService windowService, TViewModel viewModel, string title)
        {
            var windowViewModel = _windowViewModels[typeof(TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.Title = title;

            windowService.OpenWindowCentered(window);
            AddWindow(viewModel, window);

            return new Lazy<IWindow>(() => new WpfWindow(window));
        }

        public bool? OpenWindowDialog<TViewModel>(TViewModel viewModel)
        {
            return OpenWindowDialog(null, viewModel);
        }

        public bool? OpenWindowDialog<TViewModel>(object callerViewModel, TViewModel viewModel)
        {
            var windowViewModel = _windowViewModels[typeof (TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.Owner = GetViewModelWindowOrCurrent(callerViewModel);
            _windowStack.Push(window);
            AddWindow(viewModel, window);

            try
            {
                return window.ShowDialog();
            }
            finally
            {
                _windowStack.Pop();
            }
        }

        public bool? OpenWindowDialog<TViewModel>(TViewModel viewModel, string title)
        {
            return OpenWindowDialog((object) null, viewModel, title);
        }

        public bool? OpenWindowDialog<TViewModel>(object callerViewModel, TViewModel viewModel, string title)
        {
            var windowViewModel = _windowViewModels[typeof (TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.Owner = GetViewModelWindowOrCurrent(callerViewModel);
            window.Title = title;
            _windowStack.Push(window);
            AddWindow(viewModel, window);

            try
            {
                return window.ShowDialog();
            }
            finally
            {
                _windowStack.Pop();
            }
        }

        public bool? OpenWindowServiceDialog<TViewModel>(IWindowService windowService, TViewModel viewModel, string title)
        {
            var windowViewModel = _windowViewModels[typeof(TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            window.Title = title;
            _windowStack.Push(window);
            AddWindow(viewModel, window);

            try
            {
                return windowService.OpenWindowModal(window);
            }
            finally
            {
                _windowStack.Pop();
            }
        }

        public bool? OpenWindowServiceDialog<TViewModel>(IWindowService windowService, TViewModel viewModel)
        {
            var windowViewModel = _windowViewModels[typeof(TViewModel)];
            var window = windowViewModel.GetWindow();
            window.DataContext = viewModel;
            _windowStack.Push(window);
            AddWindow(viewModel, window);

            try
            {
                return windowService.OpenWindowModal(window);
            }
            finally
            {
                _windowStack.Pop();
            }
        }

        public bool? ShowFileDialog(FileDialog fileDialog)
        {
            return ShowFileDialog(null, fileDialog);
        }

        public bool? ShowFileDialog(object viewModel, FileDialog fileDialog)
        {
            return fileDialog.ShowDialog(GetViewModelWindowOrCurrent(viewModel));
        }

        public bool? ShowDialog(ShowDialogDelegate showDialogDelegate)
        {
            return ShowDialog(null, showDialogDelegate);
        }

        public bool? ShowDialog(object viewModel, ShowDialogDelegate showDialogDelegate)
        {
            var window = GetViewModelWindowOrCurrent(viewModel);
            return showDialogDelegate(window);
        }

        public MessageBoxResult ShowMessageBox(string text)
        {
            return ShowMessageBox((object)null, text);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption)
        {
            return ShowMessageBox(null, text, caption);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons)
        {
            return ShowMessageBox(null, text, caption, buttons);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            return ShowMessageBox(null, text, caption, buttons, icon);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult)
        {
            return ShowMessageBox(null, text, caption, buttons, icon, defResult);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options)
        {
            return ShowMessageBox(null, text, caption, buttons, icon, defResult, options);
        }

        public MessageBoxResult ShowMessageBox(object viewModel, string text)
        {
            return MessageBoxEx.Show(GetViewModelWindowOrCurrent(viewModel), text);
        }

        public MessageBoxResult ShowMessageBox(object viewModel, string text, string caption)
        {
            return MessageBoxEx.Show(GetViewModelWindowOrCurrent(viewModel), text, caption);
        }

        public MessageBoxResult ShowMessageBox(object viewModel, string text, string caption, MessageBoxButton buttons)
        {
            return MessageBoxEx.Show(GetViewModelWindowOrCurrent(viewModel), text, caption, buttons);
        }

        public MessageBoxResult ShowMessageBox(object viewModel, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            return MessageBoxEx.Show(GetViewModelWindowOrCurrent(viewModel), text, caption, buttons, icon);
        }

        public MessageBoxResult ShowMessageBox(object viewModel, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult)
        {
            return MessageBoxEx.Show(GetViewModelWindowOrCurrent(viewModel), text, caption, buttons, icon, defResult);
        }

        public MessageBoxResult ShowMessageBox(object viewModel,string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options)
        {
            return MessageBoxEx.Show(GetViewModelWindowOrCurrent(viewModel), text, caption, buttons, icon, defResult, options);
        }

        private Window GetViewModelWindowOrCurrent(object viewModel)
        {
            Window window;
            if (viewModel == null || !_allWindows.TryGetValue(viewModel, out window))
                return _windowStack.Count > 0 ? _windowStack.Peek() : null;

            return window;
        }

        private void AddWindow(object viewModel, Window window)
        {
            _allWindows.Add(viewModel, window);
            window.Closed += (sender, args) => _allWindows.Remove(viewModel);
        }

        private interface IWindowViewModel
        {
            Window GetWindow();
        }

        private class WindowViewModel<TWindow> : IWindowViewModel where TWindow : Window, new()
        {
            public Window GetWindow()
            {
                return new TWindow();
            }
        }

        private interface IViewViewModel
        {
            FrameworkElement GetView();
        }

        private class ViewViewModel<TView> : IViewViewModel where TView : FrameworkElement, new()
        {
            public FrameworkElement GetView()
            {
                return new TView();
            }
        }
    }

    public class WpfWindow : IWindow, INotifyPropertyChanged
    {
        private readonly IFullscreenableWindow _fullscreenableWindow;

        public WpfWindow(Window window)
        {
            Window = window;
            window.AddValueChanged(Window.TopmostProperty, TopmostChanged);

            _fullscreenableWindow = window as IFullscreenableWindow;
            if (_fullscreenableWindow != null)
            {
                CanBeFullscreen = true;
                _fullscreenableWindow.FullscreenChanged += FullscreenableWindowOnFullscreenChanged;
            }
        }

        private void FullscreenableWindowOnFullscreenChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(IsFullscreen));
        }

        private void TopmostChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(IsTopmost));
        }

        public Window Window { get; }
        public bool CanBeFullscreen { get; }

        public event EventHandler Closed
        {
            add { Window.Closed += value; }
            remove { Window.Closed -= value; }
        }

        public event CancelEventHandler Closing
        {
            add { Window.Closing += value; }
            remove { Window.Closing -= value; }
        }

        public void Activate()
        {
            Window.Activate();
        }

        public void Close()
        {
            Window.Close();
        }

        public bool IsFullscreen
        {
            get { return _fullscreenableWindow?.IsFullscreen ?? false; }
            set
            {
                if (_fullscreenableWindow != null && _fullscreenableWindow.IsFullscreen != value)
                {
                    _fullscreenableWindow.IsFullscreen = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTopmost
        {
            get { return Window.Topmost; }
            set
            {
                if (value != Window.Topmost)
                {
                    Window.Topmost = value;
                    OnPropertyChanged();
                }
            }
        }

        public WindowState WindowState
        {
            get { return Window.WindowState; }
            set { Window.WindowState = value; }
        }

        public bool IsExternalWindow => Window == Application.Current.MainWindow;

        public bool? OpenWindowModal(Window window)
        {
            window.Owner = Window;
            return window.ShowDialog();
        }

        public void OpenWindowCentered(Window window)
        {
            window.CenterOnWindow(Window);
            window.Show();
        }

        public bool? ShowDialog(ShowDialogDelegate showDialogDelegate)
        {
            return showDialogDelegate(Window);
        }

        public MessageBoxResult ShowMessageBox(string text)
        {
          return  MessageBoxEx.Show(Window, text);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption)
        {
          return  MessageBoxEx.Show(Window, text, caption);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons)
        {
            return MessageBoxEx.Show(Window, text, caption, buttons);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBoxEx.Show(Window, text, caption, buttons, icon);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon,
            MessageBoxResult defResult)
        {
            return MessageBoxEx.Show(Window, text, caption, buttons, icon, defResult);
        }

        public MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon,
            MessageBoxResult defResult, MessageBoxOptions options)
        {
            return MessageBoxEx.Show(Window, text, caption, buttons, icon, defResult, options);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}