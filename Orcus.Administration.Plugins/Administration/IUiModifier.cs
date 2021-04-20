using System;
using System.Windows.Controls;
using Orcus.Administration.Plugins.DataManager;

namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     Modifies the User Interface of the administration
    /// </summary>
    public interface IUiModifier
    {
        /// <summary>
        ///     Add a menu item to the main menu of Orcus within the menuitem "Plugins"
        /// </summary>
        /// <param name="menuItem">The menu item to add</param>
        /// <param name="menuEventHandler">The event gets fired if the user clicks on the item</param>
        void AddMainMenuItem(MenuItem menuItem, EventHandler<MenuItemClickedEventArgs> menuEventHandler);

        /// <summary>
        ///     Add a menu item to the context menu of an offline client
        /// </summary>
        /// <param name="menuItem">The menu item to add</param>
        /// <param name="menuEventHandler">The event gets fired if the user clicks on the item</param>
        void AddOfflineClientMenuItem(MenuItem menuItem,
            EventHandler<OfflineClientMenuItemClickedEventArgs> menuEventHandler);

        /// <summary>
        ///     Add a menu item to the context menu of an online client
        /// </summary>
        /// <param name="menuItem">The menu item to add</param>
        /// <param name="menuEventHandler">The event gets fired if the user clicks on the item</param>
        void AddOnlineClientMenuItem(MenuItem menuItem,
            EventHandler<OnlineClientMenuItemClickedEventArgs> menuEventHandler);

        /// <summary>
        ///     Add a new data manager type
        /// </summary>
        /// <param name="dataManagerType">The data manager type to add</param>
        void AddDataManagerType(IDataManagerType dataManagerType);
    }
}