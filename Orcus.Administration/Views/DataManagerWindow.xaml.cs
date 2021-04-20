using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.ViewModels;
using Orcus.Administration.ViewModels.DataManager;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for DataManagerWindow.xaml
    /// </summary>
    public partial class DataManagerWindow : INotifyPropertyChanged
    {
        private bool _isEnoughSpaceForSplitView = true;

        public DataManagerWindow()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            DataContextChanged += OnDataContextChanged;

            IsEnoughSpaceForSplitView = false;
        }

        public bool IsEnoughSpaceForSplitView
        {
            get { return _isEnoughSpaceForSplitView; }
            set
            {
                if (_isEnoughSpaceForSplitView != value)
                {
                    _isEnoughSpaceForSplitView = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        RootGrid.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(1, GridUnitType.Pixel)
                        });
                        RootGrid.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });
                        RootGrid.ColumnDefinitions[0].MaxWidth = 1000;
                    }
                    else
                    {
                        RootGrid.ColumnDefinitions.RemoveAt(1);
                        RootGrid.ColumnDefinitions.RemoveAt(1);
                        RootGrid.ColumnDefinitions[0].MaxWidth = double.PositiveInfinity;
                    }

                    var viewModel = DataContext as DataManagerViewModel;
                    if (viewModel != null)
                        viewModel.IsSplitViewOpened = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnDataContextChanged(object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var viewModel = dependencyPropertyChangedEventArgs.NewValue as DataManagerViewModel;
            if (viewModel != null)
                viewModel.IsSplitViewOpened = IsEnoughSpaceForSplitView;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            IsEnoughSpaceForSplitView = sizeChangedEventArgs.NewSize.Width > 1000 && DataListView.SelectedItem != null &&
                                        ((ViewData) DataListView.SelectedItem).DataManagerType.IsDataViewable;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsEnoughSpaceForSplitView = ActualWidth > 1000 && DataListView.SelectedItem != null &&
                                        ((ViewData) DataListView.SelectedItem).DataManagerType.IsDataViewable;
            ((DataManagerViewModel) DataContext).SelectedItemsChanged(
                ((ListBox) sender).SelectedItems.Cast<ViewData>().ToList());
        }
    }
}