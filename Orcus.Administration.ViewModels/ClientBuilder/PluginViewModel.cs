using System;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public abstract class PluginViewModel : PropertyChangedBase
    {
        private bool _isSelected;
        private RelayCommand _openSettingsCommand;

        protected PluginViewModel(IPlugin plugin, object pluginObject, bool isBuildPlugin)
        {
            PluginObject = pluginObject;
            if (pluginObject is IProvideEditableProperties)
                PluginSettings = PluginSettings.PropertyGrid;
            else if (pluginObject is IProvideBuilderSettings)
                PluginSettings = PluginSettings.BuilderSettings;
            else if (pluginObject is IProvideWindowSettings)
                PluginSettings = PluginSettings.Window;

            Plugin = plugin;
            IsBuildPlugin = isBuildPlugin;
        }

        public event EventHandler<OpenPluginSettingsEventArgs> OpenSettings;
        public event EventHandler SelectedChanged;

        public long Size { get; set; }
        public bool IsBuildPlugin { get; }
        public IPlugin Plugin { get; }
        public object PluginObject { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (SetProperty(value, ref _isSelected))
                    SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public PluginSettings PluginSettings { get; }

        public RelayCommand OpenSettingsCommand
        {
            get
            {
                return _openSettingsCommand ??
                       (_openSettingsCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   OpenSettings?.Invoke(this, new OpenPluginSettingsEventArgs(Plugin, PluginObject));
                               }));
            }
        }
    }
}