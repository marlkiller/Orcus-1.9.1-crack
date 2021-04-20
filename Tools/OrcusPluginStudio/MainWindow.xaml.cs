using System;
using OrcusPluginStudio.Core;
using OrcusPluginStudio.Core.Settings;
using OrcusPluginStudio.ViewModels;
using OrcusPluginStudio.Views;

namespace OrcusPluginStudio
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly OrcusPluginProject _orcusPluginProject;
        private readonly string _path;
        private bool _initialized;

        public MainWindow(OrcusPluginProject orcusPluginProject, string path)
        {
            _orcusPluginProject = orcusPluginProject;
            _path = path;
            InitializeComponent();
            OrcusPluginStudioSettings.LoadSettings();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (!_initialized)
            {
                _initialized = true;

                if (_orcusPluginProject != null && !string.IsNullOrEmpty(_path))
                {
                    DataContext = new MainViewModel(_orcusPluginProject, _path);
                    return;
                }

                var window = new WelcomeWindow {Owner = this};
                if (window.ShowDialog() == true)
                {
                    var viewModel = new MainViewModel(window.PluginProject, window.PluginPath);
                    DataContext = viewModel;
                    viewModel.NewOpenEvent += ViewModel_NewOpenEvent;
                }
                else Close();
            }
        }

        private void ViewModel_NewOpenEvent(object sender, EventArgs e)
        {
            var window = new WelcomeWindow {Owner = this};
            if (window.ShowDialog() == true)
            {
                var viewModel = new MainViewModel(window.PluginProject, window.PluginPath);
                DataContext = viewModel;
                viewModel.NewOpenEvent += ViewModel_NewOpenEvent;
            }
        }
    }
}