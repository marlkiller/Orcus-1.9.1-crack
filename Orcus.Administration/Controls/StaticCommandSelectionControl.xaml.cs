using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Plugins.StaticCommands;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for StaticCommandSelectionControl.xaml
    /// </summary>
    public partial class StaticCommandSelectionControl
    {
        public static readonly DependencyProperty SelectedStaticCommandProperty = DependencyProperty.Register(
            "SelectedStaticCommand", typeof (StaticCommand), typeof (StaticCommandSelectionControl),
            new FrameworkPropertyMetadata(default(StaticCommand)) {BindsTwoWayByDefault = true});

        public static readonly DependencyProperty ShowActiveCommandsProperty = DependencyProperty.Register(
            "ShowActiveCommands", typeof(bool), typeof(StaticCommandSelectionControl), new PropertyMetadata(true, ShowActiveCommandsPropertyChangedCallback));

        private static void ShowActiveCommandsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((StaticCommandSelectionControl) dependencyObject).StaticCommandsCollectionView.Refresh();
        }

        public bool ShowActiveCommands
        {
            get { return (bool) GetValue(ShowActiveCommandsProperty); }
            set { SetValue(ShowActiveCommandsProperty, value); }
        }

        private string _searchText;

        public StaticCommandSelectionControl()
        {
            InitializeComponent();
            StaticCommandsCollectionView = new CollectionViewSource {Source = StaticCommander.GetStaticCommands()}.View;
            StaticCommandsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            StaticCommandsCollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            StaticCommandsCollectionView.Filter = FilterCommands;

            Loaded += OnLoaded;
        }

        public ICollectionView StaticCommandsCollectionView { get; }

        public StaticCommand SelectedStaticCommand
        {
            get { return (StaticCommand) GetValue(SelectedStaticCommandProperty); }
            set { SetValue(SelectedStaticCommandProperty, value); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    StaticCommandsCollectionView.Refresh();
                }
            }
        }

        private bool FilterCommands(object o)
        {
            var staticCommand = (StaticCommand) o;
            if (!ShowActiveCommands && staticCommand is ActiveStaticCommand)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            return staticCommand.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) > -1;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            SelectedStaticCommand = StaticCommandsCollectionView.Cast<StaticCommand>().FirstOrDefault();
        }
    }
}