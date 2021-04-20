using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Plugins.Administration;
using Orcus.Administration.Plugins.DataManager;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Core.Plugins
{
    public class UiModifier : IUiModifier
    {
        private readonly Dictionary<MenuItem, EventHandler<MenuItemClickedEventArgs>> _mainMenuEventHandlers;

        private readonly Dictionary<MenuItem, EventHandler<OfflineClientMenuItemClickedEventArgs>>
            _offlineClientMenuEventHandlers;

        private readonly Dictionary<MenuItem, EventHandler<OnlineClientMenuItemClickedEventArgs>>
            _onlineClientMenuEventHandler;

        public UiModifier()
        {
            DataManagerTypes = new List<IDataManagerType>();
            MainMenuItems = new ObservableCollection<MenuItem>();
            OfflineClientMenuItems = new ObservableCollection<MenuItem>();
            OnlineClientMenuItems = new ObservableCollection<MenuItem>();

            _mainMenuEventHandlers = new Dictionary<MenuItem, EventHandler<MenuItemClickedEventArgs>>();
            _offlineClientMenuEventHandlers =
                new Dictionary<MenuItem, EventHandler<OfflineClientMenuItemClickedEventArgs>>();
            _onlineClientMenuEventHandler =
                new Dictionary<MenuItem, EventHandler<OnlineClientMenuItemClickedEventArgs>>();
        }

        public ObservableCollection<MenuItem> MainMenuItems { get; }
        public ObservableCollection<MenuItem> OfflineClientMenuItems { get; }
        public ObservableCollection<MenuItem> OnlineClientMenuItems { get; }
        public List<IDataManagerType> DataManagerTypes { get;}

        public void AddMainMenuItem(MenuItem menuItem, EventHandler<MenuItemClickedEventArgs> menuEventHandler)
        {
            _mainMenuEventHandlers.Add(menuItem, menuEventHandler);
            MainMenuItems.Add(menuItem);
        }

        public void AddOfflineClientMenuItem(MenuItem menuItem,
            EventHandler<OfflineClientMenuItemClickedEventArgs> menuEventHandler)
        {
            _offlineClientMenuEventHandlers.Add(menuItem, menuEventHandler);
            OfflineClientMenuItems.Add(menuItem);
        }

        public void AddOnlineClientMenuItem(MenuItem menuItem,
            EventHandler<OnlineClientMenuItemClickedEventArgs> menuEventHandler)
        {
            _onlineClientMenuEventHandler.Add(menuItem, menuEventHandler);
            OnlineClientMenuItems.Add(menuItem);
        }

        public void AddDataManagerType(IDataManagerType dataManagerType)
        {
            DataManagerTypes.Add(dataManagerType);
        }

        public void MainMenuItemClicked(MenuItem menuItem)
        {
            if (_mainMenuEventHandlers.ContainsKey(menuItem))
                _mainMenuEventHandlers[menuItem].Invoke(this,
                    new MenuItemClickedEventArgs(Application.Current.MainWindow));
        }

        public void OnlineClientInformationMenuItemClicked(MenuItem menuItem, OnlineClientInformation clientInformation)
        {
            if (_onlineClientMenuEventHandler.ContainsKey(menuItem))
                _onlineClientMenuEventHandler[menuItem].Invoke(this,
                    new OnlineClientMenuItemClickedEventArgs(Application.Current.MainWindow, clientInformation));
        }

        public void OfflineClientInformationMenuItemClicked(MenuItem menuItem,
            OfflineClientInformation clientInformation)
        {
            if (_offlineClientMenuEventHandlers.ContainsKey(menuItem))
                _offlineClientMenuEventHandlers[menuItem].Invoke(this,
                    new OfflineClientMenuItemClickedEventArgs(Application.Current.MainWindow, clientInformation));
        }
    }
}