using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Utilities;
using Orcus.Plugins.Builder;
using Orcus.Shared.Client;
using Orcus.Shared.Core;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuilderSettingsManager : PropertyChangedBase
    {
        private readonly Dictionary<IPlugin, List<IBuilderProperty>> _externalBuilderProperties;
        private readonly List<BuilderSettingsOverwrite> _overwrittenProperties;
        private ObservableCollection<BuilderPropertyViewModel> _builderSettings;
        private ReadOnlyCollection<IBuilderProperty> _coreSettings;

        public BuilderSettingsManager()
        {
            _coreSettings = new ReadOnlyCollection<IBuilderProperty>(BuilderPropertyHelper.GetAllBuilderProperties());

            BuilderSettings =
                new ObservableCollection<BuilderPropertyViewModel>(
                    _coreSettings.Select(x => new BuilderPropertyViewModel(x)));
            _overwrittenProperties = new List<BuilderSettingsOverwrite>();
            _externalBuilderProperties = new Dictionary<IPlugin, List<IBuilderProperty>>();
        }

        public ObservableCollection<BuilderPropertyViewModel> BuilderSettings
        {
            get { return _builderSettings; }
            set { SetProperty(value, ref _builderSettings); }
        }

        public event EventHandler<BuilderSettingsOverwriteRequestEventArgs> BuilderSettingsOverwriteRequest;

        public void AddOverwritingPlugin(IPlugin plugin, IOverwriteBuilderProperties overwriteBuilderProperties)
        {
            foreach (var builderProperty in overwriteBuilderProperties.OverwrittenSettings)
            {
                var builderProperType = builderProperty.GetType();
                int builderPropertyIndex = -1;

                for (int i = 0; i < _coreSettings.Count; i++)
                {
                    if (_coreSettings[i].GetType() == builderProperType)
                    {
                        builderPropertyIndex = i;
                        break;
                    }
                }

                if (builderPropertyIndex == -1)
                    throw new InvalidOperationException(
                        $"{plugin.PluginInfo.Name} tried to overwrite a builder property of type {builderProperType} which wasn't found in the default builder properties");

                var builderOverwrite =
                    _overwrittenProperties.FirstOrDefault(x => x.PropertyIndex == builderPropertyIndex);

                var currentBuilderOverwrite = new BuilderSettingsOverwrite(builderPropertyIndex, builderProperty, plugin);

                _overwrittenProperties.Add(currentBuilderOverwrite);

                if (builderOverwrite != null)
                {
                    var builderSettingsOverwriteRequestEventArgs =
                        new BuilderSettingsOverwriteRequestEventArgs(builderOverwrite.Plugin, plugin);

                    BuilderSettingsOverwriteRequest?.Invoke(this, builderSettingsOverwriteRequestEventArgs);
                    if (builderSettingsOverwriteRequestEventArgs.Overwrite)
                        _overwrittenProperties.Swap(builderOverwrite, currentBuilderOverwrite);
                }
            }

            ApplyOverwrites();
        }

        public void AddProvidingPlugin(IPlugin plugin, IProvideBuilderSettings provideBuilderSettings)
        {
            _externalBuilderProperties.Add(plugin,
                provideBuilderSettings.BuilderSettings.Select(x => x.BuilderProperty).ToList());

            foreach (var builderPropertyEntry in provideBuilderSettings.BuilderSettings)
            {
                BuilderSettings.Add(new BuilderPropertyViewModel(builderPropertyEntry.BuilderProperty)
                {
                    IsFromPlugin = true
                });
            }
        }

        public void RemovePlugin(IPlugin plugin)
        {
            for (int i = _overwrittenProperties.Count - 1; i >= 0; i--)
            {
                if (_overwrittenProperties[i].Plugin == plugin)
                    _overwrittenProperties.RemoveAt(i);
            }

            List<IBuilderProperty> pluginBuilderProperties;
            if (_externalBuilderProperties.TryGetValue(plugin, out pluginBuilderProperties))
            {
                foreach (var pluginBuilderProperty in pluginBuilderProperties)
                {
                    var builderSettingsEntry =
                        BuilderSettings.FirstOrDefault(x => x.BuilderProperty == pluginBuilderProperty);
                    if (builderSettingsEntry != null)
                        BuilderSettings.Remove(builderSettingsEntry);
                }

                _externalBuilderProperties.Remove(plugin);
            }

            ApplyOverwrites();
        }

        public void InitializeSettings(IList<IBuilderProperty> settings)
        {
            var newSettings = new List<IBuilderProperty>(BuilderPropertyHelper.GetAllBuilderProperties());
            for (int i = 0; i < newSettings.Count; i++)
            {
                var builderProperty = newSettings[i];
                var builderPropertyType = builderProperty.GetType();
                var existingSetting = settings.FirstOrDefault(x => x.GetType() == builderPropertyType);

                if (existingSetting != null)
                    newSettings[i] = existingSetting;

                BuilderSettings[i] = new BuilderPropertyViewModel(existingSetting ?? builderProperty);
            }

            _coreSettings = new ReadOnlyCollection<IBuilderProperty>(newSettings);
            ApplyOverwrites();
        }

        public void UpdatePluginSettings(IPlugin plugin, List<PluginSetting> pluginSettings)
        {
            List<IBuilderProperty> pluginBuilderProperties;
            if (!_externalBuilderProperties.TryGetValue(plugin, out pluginBuilderProperties))
                throw new Exception("Build plugin does not exist");

            foreach (var pluginSetting in pluginSettings)
            {
                var builderSetting =
                    BuilderSettings.FirstOrDefault(
                        x =>
                            x.BuilderProperty.GetType().GetClientSettingTypeName() ==
                            pluginSetting.SettingsType);
                if (builderSetting != null)
                {
                    BuilderSettings.Remove(builderSetting);
                    pluginBuilderProperties.Remove(builderSetting.BuilderProperty);

                    var builderProperty = builderSetting.BuilderProperty.Clone();
                    BuilderPropertyHelper.ApplyProperties(
                        builderProperty, pluginSetting.Properties);
                    BuilderPropertyHelper.ApplyProperties(
                        builderSetting.BuilderProperty, pluginSetting.Properties);
                        //i know that this is ugly af, but actually these are new setting; the problem is if the user deactivates and activates the plugin because the settings from the plugin will be used and not the settings from the configuration. Perhaps I'll have to change that later
                    BuilderSettings.Add(
                        new BuilderPropertyViewModel(builderProperty)
                        {
                            IsFromPlugin = true
                        });
                    pluginBuilderProperties.Add(builderProperty);
                }
            }

            ApplyOverwrites();
        }

        private void ApplyOverwrites()
        {
            for (int i = 0; i < _coreSettings.Count; i++)
            {
                var customBuilderProperty = _overwrittenProperties.FirstOrDefault(x => x.PropertyIndex == i);

                var newBuilderProperty = customBuilderProperty != null
                    ? customBuilderProperty.NewBuilderProperty
                    : _coreSettings[i];
                if (BuilderSettings[i].BuilderProperty != newBuilderProperty)
                    BuilderSettings[i] = new BuilderPropertyViewModel(newBuilderProperty)
                    {
                        IsEnabled = customBuilderProperty == null
                    };
            }
        }
    }
}