using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels;
using Orcus.Administration.ViewModels.Controller;
using Orcus.Administration.Views.CommandViews;
using IViewPlugin = Orcus.Administration.Core.Plugins.IViewPlugin;

#if DEBUG
using System.Diagnostics;
#endif

namespace Orcus.Administration.Views
{
    public class ViewManager : DependencyObject, IValueConverter, IViewManagerModelController
    {
        private readonly Dictionary<Type, FrameworkElement> _cachedViews;
        private readonly ReadOnlyDictionary<Type, IViewModelConnector> _viewModels;

        public ViewManager()
        {
            _cachedViews = new Dictionary<Type, FrameworkElement>();
            var viewModels =
                new List<IViewModelConnector>(
                    PluginManager.Current.LoadedPlugins.OfType<IViewPlugin>()
                        .Select(x => new ViewModelConnector(x.CommandView, x.ViewType)))
                {
                    new GenericViewModelConnector<AudioViewModel, AudioCommandView>(),
                    new GenericViewModelConnector<DeviceManagerViewModel, DeviceManagerCommandView>(),
                    new GenericViewModelConnector<FileExplorerViewModel, FileExplorerCommandView>(),
                    new GenericViewModelConnector<LivePerformanceViewModel, LivePerformanceCommandView>(),
                    new GenericViewModelConnector<ClientPluginsViewModel, ClientPluginsCommandView>(),
                    new GenericViewModelConnector<RegistryViewModel, RegistryCommandView>(),
                    new GenericViewModelConnector<ActiveConnectionsViewModel, ActiveConnectionsCommandView>(),
                    new GenericViewModelConnector<AudioVolumeControlViewModel, AudioVolumeControlCommandView>(),
                    new GenericViewModelConnector<ClientCommandsViewModel, ClientCommandsCommandView>(),
                    new GenericViewModelConnector<ClientConfigViewModel, ClientConfigCommandView>(),
                    new GenericViewModelConnector<ClientControlViewModel, ClientControlCommandView>(),
                    new GenericViewModelConnector<CodeViewModel, CodeCommandView>(),
                    new GenericViewModelConnector<ComputerInformationViewModel, ComputerInformationCommandView>(),
                    new GenericViewModelConnector<ConsoleViewModel, ConsoleCommandView>(),
                    new GenericViewModelConnector<EventLogViewModel, EventLogCommandView>(),
                    new GenericViewModelConnector<FunViewModel, FunCommandView>(),
                    new GenericViewModelConnector<DropAndExecuteViewModel, DropAndExecuteCommandView>(),
                    new GenericViewModelConnector<WindowsDriversViewModel, WindowsDriversCommandView>(),
                    new GenericViewModelConnector<LiveKeyloggerViewModel, LiveKeyloggerCommandView>(),
                    new GenericViewModelConnector<MessageBoxViewModel, MessageBoxCommandView>(),
                    new GenericViewModelConnector<PasswordsViewModel, PasswordsCommandView>(),
                    new GenericViewModelConnector<RemoteDesktopViewModel, RemoteDesktopCommandView>(),
                    new GenericViewModelConnector<ReverseProxyViewModel, ReverseProxyCommandView>(),
                    new GenericViewModelConnector<StartupManagerViewModel, StartupManagerCommandView>(),
                    new GenericViewModelConnector<SystemRestoreViewModel, SystemRestoreCommandView>(),
                    new GenericViewModelConnector<TaskmanagerViewModel, TaskManagerCommandView>(),
                    new GenericViewModelConnector<TextChatViewModel, TextChatCommandView>(),
                    new GenericViewModelConnector<UninstallProgramsViewModel, UninstallProgramsCommandView>(),
                    new GenericViewModelConnector<UserInteractionViewModel, UserInteractionCommandView>(),
                    new GenericViewModelConnector<WebcamViewModel, WebcamCommandView>(),
                    new GenericViewModelConnector<WindowManagerViewModel, WindowManagerCommandView>(),
                    new GenericViewModelConnector<WindowsCustomizerViewModel, WindowsCustomizerCommandView>(),
                    new GenericViewModelConnector<VoiceChatViewModel, VoiceChatCommandView>(),
                    new GenericViewModelConnector<ClipboardManagerViewModel, ClipboardManagerCommandView>(),
                };

            _viewModels =
                new ReadOnlyDictionary<Type, IViewModelConnector>(viewModels.ToDictionary(x => x.ViewModelType, y => y));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var viewType = GetViewModelViewType(value);
            if (viewType == null)
            {
#if DEBUG
                Debug.Fail("View was not found");
#else
                return null;
#endif
            }

            return GetView(value.GetType(), value, viewType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        private FrameworkElement GetView(Type viewModelType, object viewModel, IViewModelConnector viewModelConnector, bool useCache = true)
        {
            if (_cachedViews.ContainsKey(viewModelType) && useCache)
                return _cachedViews[viewModelType];

            var view = viewModelConnector.GetView();
            view.DataContext = viewModel;

            ((ICommandView) viewModel).LoadView(Settings.Current.LoadCommandViewDataAutomatically);

            if (useCache)
                _cachedViews.Add(viewModelType, view);
            return view;
        }

        [CanBeNull]
        private IViewModelConnector GetViewModelViewType(object viewModel)
        {
            var viewModelType = viewModel.GetType();

            IViewModelConnector viewModelConnector;
            if (!_viewModels.TryGetValue(viewModelType, out viewModelConnector))
                return null;

            return viewModelConnector;
        }

        public FrameworkElement GetView(object viewModel)
        {
            var viewType = GetViewModelViewType(viewModel);
            if (viewType == null)
                return null;
            return GetView(viewModel.GetType(), viewModel, viewType);
        }

        public FrameworkElement GetNewView(object viewModel)
        {
            var viewType = GetViewModelViewType(viewModel);
            if (viewType == null)
                return null;
            return GetView(viewModel.GetType(), viewModel, viewType, false);
        }

        private interface IViewModelConnector
        {
            Type ViewModelType { get; }
            FrameworkElement GetView();
        }

        private class GenericViewModelConnector<TViewModel, TView> : IViewModelConnector where TViewModel : ICommandView where TView : FrameworkElement, new()
        {
            public Type ViewModelType { get; } = typeof (TViewModel);

            public FrameworkElement GetView()
            {
                return new TView();
            }
        }

        private class ViewModelConnector : IViewModelConnector
        {
            private readonly Type _viewType;

            public ViewModelConnector(Type viewModelType, Type viewType)
            {
                ViewModelType = viewModelType;
                _viewType = viewType;
            }

            public Type ViewModelType { get; }

            public FrameworkElement GetView()
            {
                return (FrameworkElement) Activator.CreateInstance(_viewType);
            }
        }
    }
}