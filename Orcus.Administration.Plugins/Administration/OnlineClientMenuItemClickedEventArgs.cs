using System.Windows;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     The user clicked on a menu item which was displayed in a context menu of an online client
    /// </summary>
    public class OnlineClientMenuItemClickedEventArgs : MenuItemClickedEventArgs
    {
        public OnlineClientMenuItemClickedEventArgs(Window window, OnlineClientInformation clientInformation)
            : base(window)
        {
            ClientInformation = clientInformation;
        }

        /// <summary>
        ///     Information about the client
        /// </summary>
        public OnlineClientInformation ClientInformation { get; set; }
    }
}