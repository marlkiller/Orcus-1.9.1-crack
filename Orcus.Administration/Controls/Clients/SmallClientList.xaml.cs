using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Core.ClientManagement;

namespace Orcus.Administration.Controls.Clients
{
    /// <summary>
    ///     Interaction logic for SmallClientList.xaml
    /// </summary>
    public partial class SmallClientList : IClientPresenter, INotifyPropertyChanged
    {
        private readonly ClientProvider _clientProvider;
        private FilterParser _currentFilter;

        private ContextMenu _itemContextMenu;
        private ICommands _commands;

        public SmallClientList(ClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
            InitializeComponent();

            CollectionView = new CollectionViewSource {Source = clientProvider.Clients};
            CollectionView.Filter += CollectionViewOnFilter;
        }

        public CollectionViewSource CollectionView { get; }

        public List<ClientViewModel> VisibleClients { get; set; }

        public ContextMenu ItemContextMenu
        {
            get { return _itemContextMenu; }
            set
            {
                if (_itemContextMenu != value)
                {
                    _itemContextMenu = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommands Commands
        {
            get { return _commands; }
            set
            {
                if (_commands != value)
                {
                    _commands = value;
                    OnPropertyChanged();
                }
            }
        }

        public IList SelectedItems => MainListView.SelectedItems;

        public void UpdateSearchText(FilterParser filterParser)
        {
            _currentFilter = filterParser;
            CollectionView.View.Refresh();
        }

        public void Enable(FilterParser filterParser)
        {
            _currentFilter = filterParser;

            if (_currentFilter != null)
                CollectionView.View.Refresh();
        }

        public void Disable()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CollectionViewOnFilter(object sender, FilterEventArgs filterEventArgs)
        {
            filterEventArgs.Accepted = true;

            if (_currentFilter == null)
                return;

            var item = (ClientViewModel) filterEventArgs.Item;
            filterEventArgs.Accepted = _currentFilter.IsAccepted(item);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}