using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.Controls.TargetPresenting
{
    /// <summary>
    ///     Interaction logic for TargetPresenter.xaml
    /// </summary>
    public partial class TargetPresenter : INotifyPropertyChanged
    {
        public static readonly DependencyProperty PossibleTargetProperty = DependencyProperty.Register(
            "PossibleTarget", typeof (PossibleTargetPresenter), typeof (TargetPresenter),
            new PropertyMetadata(default(PossibleTargetPresenter)));

        private bool _itemsSelected;


        public TargetPresenter()
        {
            InitializeComponent();
        }

        public PossibleTargetPresenter PossibleTarget
        {
            get { return (PossibleTargetPresenter) GetValue(PossibleTargetProperty); }
            set { SetValue(PossibleTargetProperty, value); }
        }

        public bool ItemsSelected
        {
            get { return _itemsSelected; }
            private set
            {
                if (_itemsSelected != value)
                {
                    _itemsSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public CommandTarget Target
        {
            get
            {
                if (DataListBox.SelectedItems.Count == 0)
                    return null;

                if (PossibleTarget is PossibleClientsPresenter)
                {
                    return CommandTarget.FromClients(DataListBox.SelectedItems.Cast<ClientViewModel>().Select(x => x.Id).ToList());
                }

                return CommandTarget.FromGroups(DataListBox.SelectedItems.Cast<string>());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DataListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelected = DataListBox.SelectedItems.Count > 0;
            OnPropertyChanged(nameof(Target));
        }
    }
}