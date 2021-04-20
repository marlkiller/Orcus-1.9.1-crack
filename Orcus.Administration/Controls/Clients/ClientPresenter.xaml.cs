using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Core.ClientManagement;

namespace Orcus.Administration.Controls.Clients
{
    /// <summary>
    ///     Interaction logic for ClientPresenter.xaml
    /// </summary>
    public partial class ClientPresenter : INotifyPropertyChanged, ICommands
    {
        public static readonly DependencyProperty ClientProviderProperty = DependencyProperty.Register(
            "ClientProvider", typeof (ClientProvider), typeof (ClientPresenter),
            new PropertyMetadata(default(ClientProvider), PropertyChangedCallback));

        public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
            "ViewMode", typeof (ViewMode), typeof (ClientPresenter),
            new PropertyMetadata(default(ViewMode), ViewModeChangedCallback));

        public static readonly DependencyProperty ItemContextMenuProperty = DependencyProperty.Register(
            "ItemContextMenu", typeof (ContextMenu), typeof (ClientPresenter),
            new PropertyMetadata(default(ContextMenu), ItemContextMenuPropertyChangedCallback));

        public static readonly DependencyProperty IsRefreshDisabledProperty = DependencyProperty.Register(
            "IsRefreshDisabled", typeof (bool), typeof (ClientPresenter),
            new PropertyMetadata(default(bool), IsRefreshDisabledPropertyChangedCallback));

        public static readonly DependencyProperty LogInCommandProperty = DependencyProperty.Register(
            "LogInCommand", typeof (ICommand), typeof (ClientPresenter), new PropertyMetadata(default(ICommand)));

        public ICommand LogInCommand
        {
            get { return (ICommand) GetValue(LogInCommandProperty); }
            set { SetValue(LogInCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register(
            "DeleteCommand", typeof (ICommand), typeof (ClientPresenter), new PropertyMetadata(default(ICommand)));

        public ICommand DeleteCommand
        {
            get { return (ICommand) GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        private IClientPresenter _currentClientPresenter;
        private FilterParser _currentFilter;
        private string _currentSearchText;
        private IClientPresenter _defaultClientList;
        private IClientPresenter _thumbnailClientList;
        private IClientPresenter _smallClientList;

        public ClientPresenter()
        {
            InitializeComponent();
        }

        public IList SelectedItemsList => _currentClientPresenter?.SelectedItems;

        public bool IsRefreshDisabled
        {
            get { return (bool) GetValue(IsRefreshDisabledProperty); }
            set { SetValue(IsRefreshDisabledProperty, value); }
        }

        public ContextMenu ItemContextMenu
        {
            get { return (ContextMenu) GetValue(ItemContextMenuProperty); }
            set { SetValue(ItemContextMenuProperty, value); }
        }

        public ClientProvider ClientProvider
        {
            get { return (ClientProvider) GetValue(ClientProviderProperty); }
            set { SetValue(ClientProviderProperty, value); }
        }

        public string CurrentSearchText
        {
            get { return _currentSearchText; }
            set
            {
                if (value != _currentSearchText)
                {
                    _currentSearchText = value;
                    OnPropertyChanged();
                    _currentFilter = FilterParser.ParseString(value);
                    _currentClientPresenter.UpdateSearchText(_currentFilter);
                }
            }
        }

        public ViewMode ViewMode
        {
            get { return (ViewMode) GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void ItemContextMenuPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var clientPresenter = (ClientPresenter) dependencyObject;

            if (clientPresenter._currentClientPresenter != null)
                clientPresenter._currentClientPresenter.ItemContextMenu =
                    (ContextMenu) dependencyPropertyChangedEventArgs.NewValue;
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var clientPresenter = (ClientPresenter) dependencyObject;
            clientPresenter.InitializeEverything((ClientProvider) dependencyPropertyChangedEventArgs.NewValue);
        }

        private static void IsRefreshDisabledPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var clientPresenter = (ClientPresenter) dependencyObject;
            clientPresenter.StateChanged((bool) dependencyPropertyChangedEventArgs.NewValue);
        }

        private static void ViewModeChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var clientPresenter = (ClientPresenter) dependencyObject;
            clientPresenter.ViewModeChanged((ViewMode) dependencyPropertyChangedEventArgs.NewValue);
        }

        private void ViewModeChanged(ViewMode newViewMode)
        {
            IClientPresenter clientPresenter;

            switch (newViewMode)
            {
                case ViewMode.DefaultList:
                    clientPresenter = _defaultClientList ??
                                      (_defaultClientList = new DefaultClientList(ClientProvider));
                    break;
                case ViewMode.Thumbnails:
                    clientPresenter = _thumbnailClientList ??
                                      (_thumbnailClientList = new ThumbnailClientList(ClientProvider));
                    break;
                case ViewMode.SmallList:
                    clientPresenter = _smallClientList ?? (_smallClientList = new SmallClientList(ClientProvider));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newViewMode), newViewMode, null);
            }

            if (!IsRefreshDisabled)
                clientPresenter.Enable(_currentFilter);
            clientPresenter.ItemContextMenu = ItemContextMenu;
            clientPresenter.Commands = this;

            _currentClientPresenter?.Disable();
            _currentClientPresenter = clientPresenter;
            MainListContentControl.Content = clientPresenter;

            ViewMode = newViewMode;
        }

        private void InitializeEverything(ClientProvider clientProvider)
        {
            if (clientProvider == null)
            {
                MainListContentControl.Content = null;
                return;
            }

            _currentClientPresenter = null;
            _defaultClientList = null;
            _thumbnailClientList = null;

            ViewModeChanged(ViewMode.DefaultList);
        }

        private void StateChanged(bool isDisabled)
        {
            if (isDisabled)
            {
                _currentClientPresenter?.Disable();
            }
            else
            {
                _currentClientPresenter?.Enable(_currentFilter);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ViewMode
    {
        DefaultList,
        Thumbnails,
        SmallList
    }
}