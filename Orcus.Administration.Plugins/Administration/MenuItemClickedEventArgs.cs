using System;
using System.Windows;

namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     Occures when the user clicked on a menu item added with the <see cref="IUiModifier" />
    /// </summary>
    public class MenuItemClickedEventArgs : EventArgs
    {
        /// <summary>
        ///     Create a new instance of <see cref="MenuItemClickedEventArgs" />
        /// </summary>
        /// <param name="mainWindow">The main window which contains the menu item</param>
        public MenuItemClickedEventArgs(Window mainWindow)
        {
            MainWindow = mainWindow;
        }

        /// <summary>
        ///     The administration window (needed for modal dialogs)
        /// </summary>
        public Window MainWindow { get; }
    }
}