using System.Windows;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Provides methods to open windows modal to the window the command currently lives in
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        ///     Get if the window is an external window and not the main window
        /// </summary>
        bool IsExternalWindow { get; }

        /// <summary>
        ///     Get/set if the window is in fullscreen mode
        /// </summary>
        bool IsFullscreen { get; set; }

        /// <summary>
        ///     Get/set if the window is topmost
        /// </summary>
        bool IsTopmost { get; set; }

        /// <summary>
        ///     Get if the window can be made fullscreen. Only external windows will have this property set to true
        /// </summary>
        bool CanBeFullscreen { get; }

        /// <summary>
        ///     Activate the window
        /// </summary>
        void Activate();

        /// <summary>
        ///     Open the given window using <see cref="Window.ShowDialog" /> on the current window and return the result
        /// </summary>
        /// <param name="window">The window to open</param>
        /// <returns>Return the result of the window</returns>
        bool? OpenWindowModal(Window window);

        /// <summary>
        ///     Open the given window using <see cref="Window.Show" /> and position it at the center of the current window
        /// </summary>
        /// <param name="window">The window to open</param>
        void OpenWindowCentered(Window window);

        /// <summary>
        ///     Open a dialog
        /// </summary>
        /// <param name="showDialogDelegate">The dialog method</param>
        /// <example>
        ///     <code>
        /// <para />var ofd = new OpenFileDialog();
        /// <para />if (WindowService.ShowDialog(ofd.ShowDialog) == true) then
        /// <para />[...]
        /// </code>
        /// </example>
        /// <returns>Return the result of the dialog</returns>
        bool? ShowDialog(ShowDialogDelegate showDialogDelegate);

        /// <summary>
        ///     Displays a message box with the current window as owner.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        MessageBoxResult ShowMessageBox(string text);

        /// <summary>
        ///     Displays a message box with the current window as owner.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        MessageBoxResult ShowMessageBox(string text, string caption);

        /// <summary>
        ///     Displays a message box with the current window as owner.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons);

        /// <summary>
        ///     Displays a message box with the current window as owner.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon);

        /// <summary>
        ///     Displays a message box with the current window as owner.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <param name="defResult">
        ///     One of the <see cref="MessageBoxResult" /> values that specifies the default button for the
        ///     message box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon,
            MessageBoxResult defResult);

        /// <summary>
        ///     Displays a message box with the current window as owner.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <param name="defResult">
        ///     One of the <see cref="MessageBoxResult" /> values that specifies the default button for the
        ///     message box.
        /// </param>
        /// <param name="options">
        ///     One of the <see cref="MessageBoxOptions" /> values that specifies which display and association
        ///     options will be used for the message box. You may pass in 0 if you wish to use the defaults.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        MessageBoxResult ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon,
            MessageBoxResult defResult, MessageBoxOptions options);
    }

    /// <summary>
    ///     Can be any dialog method
    /// </summary>
    /// <param name="window">The owner window</param>
    /// <returns>Return the result of the dialog</returns>
    public delegate bool? ShowDialogDelegate(Window window);
}