using System.Windows;
using System.Windows.Data;
using MahApps.Metro;
using Orcus.Administration.Controls;
using Orcus.Administration.ViewModels;
using Orcus.Administration.ViewModels.Controller;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for CommandListView.xaml
    /// </summary>
    public partial class CommandListView
    {
        private bool _loaded;

        public CommandListView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void ThemeManagerOnIsThemeChanged(object sender, OnThemeChangedEventArgs onThemeChangedEventArgs)
        {
            LoadListBox();
        }

        private void LoadListBox()
        {
            var control = new CommandListControl {ItemsSource = ((ControllerViewModel) DataContext).CommandViews};

            var binding = new Binding
            {
                Path = new PropertyPath(nameof(ControllerViewModel.SelectedCommandView)),
                Source = DataContext,
                Mode = BindingMode.TwoWay
            };

            BindingOperations.SetBinding(control, CommandListControl.SelectedItemProperty, binding);

            BaseScrollViewer.Content = control;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!_loaded)
            {
                ((ControllerViewModel) DataContext).ViewManagerModelController =
                    (IViewManagerModelController) Resources["ViewManager"];
                LoadListBox();
                _loaded = true;
            }

            ThemeManager.IsThemeChanged += ThemeManagerOnIsThemeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ThemeManager.IsThemeChanged -= ThemeManagerOnIsThemeChanged;
        }
    }
}