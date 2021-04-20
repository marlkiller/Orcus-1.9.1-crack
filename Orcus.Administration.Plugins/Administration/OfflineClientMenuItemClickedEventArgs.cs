using System.Windows;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     The user clicked on a menu item which was displayed in a context menu of an offline client
    /// </summary>
    public class OfflineClientMenuItemClickedEventArgs : MenuItemClickedEventArgs
    {
        public OfflineClientMenuItemClickedEventArgs(Window window,
            OfflineClientInformation clientInformation) : base(window)
        {
            ClientInformation = clientInformation;
        }

        /// <summary>
        ///     Information about the client
        /// </summary>
        public OfflineClientInformation ClientInformation { get; set; }
    }
}