using System;
using System.Windows;
using Microsoft.Win32;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace Orcus.Administration.ViewModels.ViewInterface
{
    public interface IWindowServiceInterface
    {
        FrameworkElement GetView<TViewModel>(TViewModel viewModel);
        IWindow GetCurrentWindow();
        Lazy<IWindow> OpenWindow<TViewModel>(TViewModel viewModel);
        Lazy<IWindow> OpenWindow<TViewModel>(TViewModel viewModel, string title);
        Lazy<IWindow> OpenWindowCentered<TViewModel>(TViewModel viewModel);
        Lazy<IWindow> OpenWindowCentered<TViewModel>(IWindowService windowService, TViewModel viewModel);
        Lazy<IWindow> OpenWindowCentered<TViewModel>(TViewModel viewModel, string title);
        Lazy<IWindow> OpenWindowCentered<TViewModel>(IWindowService windowService, TViewModel viewModel, string title);
        bool? OpenWindowDialog<TViewModel>(TViewModel viewModel);
        bool? OpenWindowDialog<TViewModel>(object callerViewModel, TViewModel viewModel);
        bool? OpenWindowDialog<TViewModel>(TViewModel viewModel, string title);
        bool? OpenWindowDialog<TViewModel>(object callerViewModel, TViewModel viewModel, string title);
        bool? OpenWindowServiceDialog<TViewModel>(IWindowService windowService, TViewModel viewModel, string title);
        bool? OpenWindowServiceDialog<TViewModel>(IWindowService windowService, TViewModel viewModel);
        bool? ShowFileDialog(FileDialog fileDialog);
        bool? ShowFileDialog(object viewModel, FileDialog fileDialog);
        bool? ShowDialog(ShowDialogDelegate showDialogDelegate);
        bool? ShowDialog(object viewModel, ShowDialogDelegate showDialogDelegate);

        MessageBoxResult ShowMessageBox(string text);
        MessageBoxResult ShowMessageBox(string text, string caption);
        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons);

        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon);

        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult);

        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options);

        MessageBoxResult ShowMessageBox(object viewModel, string text);
        MessageBoxResult ShowMessageBox(object viewModel, string text, string caption);
        MessageBoxResult ShowMessageBox(object viewModel, string text, string caption, MessageBoxButton buttons);

        MessageBoxResult ShowMessageBox(object viewModel, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon);

        MessageBoxResult ShowMessageBox(object viewModel, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult);

        MessageBoxResult ShowMessageBox(object viewModel, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options);
    }
}