using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.ViewModels.ClientBuilder;
using Orcus.Plugins.Builder;
using Orcus.Shared.Core;

namespace Orcus.Administration.Controls.Builder
{
    /// <summary>
    ///     Interaction logic for BuilderGroupControl.xaml
    /// </summary>
    public partial class BuilderGroupControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty BuilderPropertiesProperty = DependencyProperty.Register(
            "BuilderProperties", typeof (ObservableCollection<BuilderPropertyViewModel>), typeof (BuilderGroupControl),
            new PropertyMetadata(default(ObservableCollection<BuilderPropertyViewModel>),
                BuilderPropertiesPropertyChangedCallback));

        public static readonly DependencyProperty BuilderCategoryProperty = DependencyProperty.Register(
            "BuilderCategory", typeof (BuilderCategory), typeof (BuilderGroupControl),
            new PropertyMetadata(default(BuilderCategory)));

        public static readonly DependencyProperty BuilderGroupProperty = DependencyProperty.Register(
            "BuilderGroup", typeof (BuilderGroup), typeof (BuilderGroupControl),
            new PropertyMetadata(default(BuilderGroup)));

        private IBuilderProperty _groupLeaderProperty;
        private ILeaderBuilderPropertyView _groupLeaderView;

        public BuilderGroupControl()
        {
            InitializeComponent();
        }

        public ObservableCollection<BuilderPropertyViewModel> BuilderProperties
        {
            get { return (ObservableCollection<BuilderPropertyViewModel>) GetValue(BuilderPropertiesProperty); }
            set { SetValue(BuilderPropertiesProperty, value); }
        }

        public BuilderCategory BuilderCategory
        {
            get { return (BuilderCategory) GetValue(BuilderCategoryProperty); }
            set { SetValue(BuilderCategoryProperty, value); }
        }

        public BuilderGroup BuilderGroup
        {
            get { return (BuilderGroup) GetValue(BuilderGroupProperty); }
            set { SetValue(BuilderGroupProperty, value); }
        }

        public ILeaderBuilderPropertyView GroupLeaderView
        {
            get { return _groupLeaderView; }
            set
            {
                if (_groupLeaderView != value)
                {
                    _groupLeaderView = value;
                    OnPropertyChanged();
                }
            }
        }

        public IBuilderProperty GroupLeaderProperty
        {
            get { return _groupLeaderProperty; }
            set
            {
                if (_groupLeaderProperty != value)
                {
                    _groupLeaderProperty = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void BuilderPropertiesPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var builderGroupControl = (BuilderGroupControl) dependencyObject;

            builderGroupControl.UpdateBuilderProperties(
                dependencyPropertyChangedEventArgs.NewValue as ObservableCollection<BuilderPropertyViewModel>,
                dependencyPropertyChangedEventArgs.OldValue as ObservableCollection<BuilderPropertyViewModel>);

            builderGroupControl.UpdateLeader();
        }

        private void UpdateBuilderProperties(ObservableCollection<BuilderPropertyViewModel> newProperties,
            ObservableCollection<BuilderPropertyViewModel> oldProperties)
        {
            if (oldProperties != null)
                oldProperties.CollectionChanged -= CollectionOnCollectionChanged;

            if (newProperties != null)
                newProperties.CollectionChanged += CollectionOnCollectionChanged;

            UpdateLeader();
        }

        private void CollectionOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdateLeader();
        }

        private void UpdateLeader()
        {
            if (BuilderProperties == null)
                GroupLeaderView = null;

            GroupLeaderView =
                (ILeaderBuilderPropertyView) BuilderPropertiesItemsControl.BuilderPropertyViews.Value.FirstOrDefault(
                    x =>
                        x.Value.PropertyPosition.IsGroupLeader &&
                        x.Value.PropertyPosition.BuilderCategory == BuilderCategory &&
                        x.Value.PropertyPosition.BuilderGroup == BuilderGroup).Value;

            var builderViewModel =
                BuilderProperties?.FirstOrDefault(x => x.BuilderProperty.GetType() == GroupLeaderView?.BuilderProperty);
            if (builderViewModel != null)
            {
                GroupLeaderProperty = builderViewModel.BuilderProperty;
                builderViewModel.BuilderPropertyView = GroupLeaderView;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}