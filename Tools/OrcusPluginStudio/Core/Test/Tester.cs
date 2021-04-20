using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.AudioPlugin;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using OrcusPluginStudio.Core.Test.ManualTests;
using Command = Orcus.Administration.Plugins.CommandViewPlugin.Command;

namespace OrcusPluginStudio.Core.Test
{
    public class Tester : IDisposable
    {
        public Tester()
        {
            TestResultEntries = new ObservableCollection<TestResultEntry>();
        }

        public string Library1 { get; set; }
        public string Library2 { get; set; }

        public ObservableCollection<TestResultEntry> TestResultEntries { get; set; }
        public IManualTest ManualTest { get; set; }

        public void Dispose()
        {
            ManualTest?.Dispose();
        }

        public void Test(PluginType pluginType)
        {
            switch (pluginType)
            {
                case PluginType.Audio:
                    var audioPlugin = CreatePlugin<IAudioPlugin>(Library1);
                    if (audioPlugin == null)
                        return;

                    List<IAudioFile> audioFiles;
                    try
                    {
                        audioFiles = audioPlugin.AudioFiles.ToList();
                    }
                    catch (Exception ex)
                    {
                        TestResultEntries.Add(new TestResultEntry
                        {
                            Failed = true,
                            FullErrorText = ex.ToString(),
                            Message = "Exception when trying to get the audio files"
                        });

                        return;
                    }

                    TestResultEntries.Add(new TestResultEntry {Message = $"{audioFiles.Count} audio files found"});
                    ManualTest = new AudioPluginTest(audioPlugin, audioFiles);
                    break;
                case PluginType.Build:
                    var buildPlugin = CreatePlugin<BuildPluginBase>(Library1, false);
                    if (buildPlugin == null)
                        return;

                    var settingsImplementations = new List<string>();
                    if (buildPlugin is IProvideBuilderSettings)
                        settingsImplementations.Add("IProvideBuilderSettings");
                    if (buildPlugin is IProvideWindowSettings)
                        settingsImplementations.Add("IProvideWindowSettings");
                    if (buildPlugin is IProvideEditableProperties)
                        settingsImplementations.Add("IProvideEditableProperties");

                    if (settingsImplementations.Count > 1)
                    {
                        TestResultEntries.Add(new TestResultEntry
                        {
                            Message =
                                "More than one settings implementation. Please only implement one settings provider. Currently implemented: " +
                                string.Join(", ", settingsImplementations),
                            Failed = true
                        });
                        return;
                    }
                    if (settingsImplementations.Count == 1)
                    {
                        TestResultEntries.Add(new TestResultEntry
                        {
                            Message = $"{settingsImplementations[0]} implemented"
                        });
                    }

                    TestResultEntries.Add(new TestResultEntry {Message = "Plugin successfully initialized"});
                    ManualTest = new BuildPluginTest(buildPlugin);
                    break;
                case PluginType.Client:
                    var clientPlugin = CreatePlugin<ClientController>(Library1, true);
                    if (clientPlugin == null)
                        return;
                    ManualTest = new ClientPluginTest(clientPlugin);
                    break;
                case PluginType.CommandView:
                    var commandView = CreatePlugin<ICommandAndViewPlugin>(Library1);
                    if (commandView == null)
                        return;

                    var command = CreatePlugin<Orcus.Plugins.Command>(Library2, true);
                    if (command == null)
                        return;

                    if (TestCommandView(commandView, command))
                        ManualTest = new CommandViewTest(commandView, command.GetType());
                    break;
                case PluginType.View:
                    var viewPlugin = CreatePlugin<IViewPlugin>(Library1);
                    if (viewPlugin == null)
                        return;

                    if (viewPlugin.CommandView == null)
                    {
                        TestResultEntries.Add(new TestResultEntry {Message = "CommandView is null", Failed = true});
                        return;
                    }

                    if (viewPlugin.View == null)
                    {
                        TestResultEntries.Add(new TestResultEntry {Message = "View is null", Failed = true});
                    }

                    break;
                case PluginType.Administration:
                    var administrationPlugin = CreatePlugin<IAdministrationPlugin>(Library1);
                    if (administrationPlugin == null)
                        return;

                    break;
                case PluginType.CommandFactory:
                    var commandFactoryPlugin = CreatePlugin<ICommandAndViewPlugin>(Library1);
                    var factoryClientPlugin = CreatePlugin<IFactoryClientCommand>(Library2);

                    if (commandFactoryPlugin == null)
                        return;

                    if (factoryClientPlugin == null)
                        return;

                    if (factoryClientPlugin.Factory == null)
                    {
                        TestResultEntries.Add(new TestResultEntry
                        {
                            Message = "IFactoryClientCommand.Factory is null",
                            Failed = true
                        });
                        return;
                    }
                    TestResultEntries.Add(new TestResultEntry {Message = "IFactoryClientCommand.Factory is not null"});

                    if (factoryClientPlugin.FactoryCommandType == null)
                    {
                        TestResultEntries.Add(new TestResultEntry
                        {
                            Message = "IFactoryClientCommand.FactoryCommandType is null",
                            Failed = true
                        });
                        return;
                    }

                    object foo;
                    try
                    {
                        foo = Activator.CreateInstance(factoryClientPlugin.FactoryCommandType);
                    }
                    catch (Exception ex)
                    {
                        TestResultEntries.Add(new TestResultEntry
                        {
                            Message =
                                $"Instance of FactoryCommand ({factoryClientPlugin.FactoryCommandType.FullName}) could not be created",
                            Failed = true,
                            FullErrorText = ex.ToString()
                        });
                        return;
                    }
                    TestResultEntries.Add(new TestResultEntry
                    {
                        Message = "Instance of FactoryCommand of FactoryCommand successfully created"
                    });
                    var factoryCommand = foo as FactoryCommand;
                    if (factoryCommand == null)
                    {
                        TestResultEntries.Add(new TestResultEntry
                        {
                            Message = "FactoryCommand doesn't inherit Orcus.Plugins.FactoryCommand",
                            Failed = true
                        });
                        return;
                    }

                    if (TestCommandView(commandFactoryPlugin, factoryCommand))
                        ManualTest = new CommandViewTest(commandFactoryPlugin, factoryClientPlugin);
                    break;
                case PluginType.StaticCommand:
                    var staticCommandPlugins = CreatePlugins<StaticCommand>(Library1, true);
                    if (staticCommandPlugins == null)
                        return;

                    var staticCommandGuids = new Dictionary<Guid, string>();
                    foreach (var staticCommandPlugin in staticCommandPlugins)
                    {
                        if (string.IsNullOrWhiteSpace(staticCommandPlugin.Name))
                        {
                            TestResultEntries.Add(new TestResultEntry
                            {
                                Message = $"The name of {staticCommandPlugin.GetType().Name} is empty",
                                Failed = true
                            });
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(staticCommandPlugin.Category?.Name))
                        {
                            TestResultEntries.Add(new TestResultEntry
                            {
                                Message = $"The category of {staticCommandPlugin.GetType().Name} is null or empty",
                                Failed = true
                            });
                            return;
                        }

                        if (staticCommandGuids.ContainsKey(staticCommandPlugin.CommandId))
                        {
                            TestResultEntries.Add(new TestResultEntry
                            {
                                Message = $"The command id of {staticCommandPlugin.GetType().Name} is already used by {staticCommandGuids[staticCommandPlugin.CommandId]}",
                                Failed = true
                            });
                            return;
                        }

                        staticCommandGuids.Add(staticCommandPlugin.CommandId, staticCommandPlugin.GetType().Name);

                        TestResultEntries.Add(new TestResultEntry
                        {
                            Message = $"{staticCommandPlugin.Name} seems alright",
                        });
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pluginType), pluginType, null);
            }
        }

        private bool TestCommandView(ICommandAndViewPlugin commandView, Orcus.Plugins.Command command)
        {
            if (commandView.Command == null)
            {
                TestResultEntries.Add(new TestResultEntry {Message = "Command is null", Failed = true});
                return false;
            }

            if (commandView.CommandView == null)
            {
                TestResultEntries.Add(new TestResultEntry {Message = "CommandView is null", Failed = true});
                return false;
            }

            if (commandView.View == null)
            {
                TestResultEntries.Add(new TestResultEntry {Message = "View is null", Failed = true});
                return false;
            }

            Command adminCommand;
            try
            {
                var test = Activator.CreateInstance(commandView.Command);
                test.ToString();
                adminCommand = test as Command;
                if (adminCommand == null)
                {
                    TestResultEntries.Add(new TestResultEntry
                    {
                        Message =
                            $"\"{commandView.Command.FullName}\" doesn't inherit Orcus.Administration.Plugins.Command",
                        Failed = true
                    });
                    return false;
                }

                if (adminCommand.Identifier != command.Identifier)
                {
                    TestResultEntries.Add(new TestResultEntry
                    {
                        Message =
                            $"The ids of the commands are not equal: Administration Command ID: {adminCommand.Identifier}, Client Command ID: {command.Identifier}",
                        Failed = true
                    });
                    return false;
                }
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message = $"Instance of Command ({commandView.Command.FullName}) could not be created",
                    Failed = true,
                    FullErrorText = ex.ToString()
                });
                return false;
            }
            TestResultEntries.Add(new TestResultEntry
            {
                Message = $"Instance of Command ({commandView.Command.FullName}) successfully created"
            });
            try
            {
                adminCommand.Dispose();
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message = "Exception when trying to dispose the administration command",
                    Failed = true,
                    FullErrorText = ex.ToString()
                });
                return false;
            }

            try
            {
                var test = Activator.CreateInstance(commandView.CommandView);
                test.ToString();

                if (!(test is ICommandView))
                {
                    TestResultEntries.Add(new TestResultEntry
                    {
                        Message =
                            $"\"{commandView.CommandView.FullName}\" doesn't implement Orcus.Administration.Plugins.ICommandView",
                        Failed = true
                    });
                    return false;
                }
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message = $"Instance of CommandView ({commandView.CommandView.FullName}) could not be created",
                    Failed = true,
                    FullErrorText = ex.ToString()
                });
                return false;
            }
            TestResultEntries.Add(new TestResultEntry
            {
                Message = $"Instance of CommandView ({commandView.CommandView.FullName}) successfully created"
            });

            try
            {
                var test = Activator.CreateInstance(commandView.View);
                test.ToString();
                if (!(test is FrameworkElement))
                {
                    TestResultEntries.Add(new TestResultEntry
                    {
                        Message =
                            $"\"{commandView.CommandView.FullName}\" doesn't inherit System.Windows.FrameworkElement",
                        Failed = true
                    });
                    return false;
                }
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message = $"Instance of View ({commandView.CommandView.FullName}) could not be created",
                    Failed = true,
                    FullErrorText = ex.ToString()
                });
                return false;
            }
            TestResultEntries.Add(new TestResultEntry
            {
                Message =
                    $"Instance of CommandView ({commandView.CommandView.FullName}) successfully created; inherits FrameworkElement"
            });

            if (command.Identifier <= 1000)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message = $"Command ID is lower than or equal to 1000 ({command.Identifier})",
                    Failed = true
                });
                return false;
            }

            try
            {
                command.Dispose();
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message = "Exception when trying to dispose the client command",
                    Failed = true,
                    FullErrorText = ex.ToString()
                });
                return false;
            }

            return true;
        }

        private T CreatePlugin<T>(string path)
        {
            var assembly = Assembly.Load(File.ReadAllBytes(path));

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Debug.Print(args.Name);
                var file = new FileInfo(args.Name.Split(',')[0] + ".dll");
                if (file.Exists)
                    return Assembly.LoadFile(file.FullName);
                return null;
            };
            Type[] types;

            try
            {
                types = assembly.GetTypes();
                TestResultEntries.Add(new TestResultEntry {Message = "Assembly.GetTypes() succeed"});
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = "Assembly.GetTypes() failed"
                });

                return default(T);
            }

            Type type;
            var interfaceName = typeof (T).Name;
            try
            {
                type = types.First(x => x.GetInterface(interfaceName) != null);
                TestResultEntries.Add(new TestResultEntry {Message = $"Class \"{type.Assembly}\" found"});
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = $"Could not find a class which implements {interfaceName}"
                });
                return default(T);
            }

            T plugin;
            try
            {
                plugin = (T) Activator.CreateInstance(type);
                TestResultEntries.Add(new TestResultEntry {Message = "Instance successfully created"});
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = "Could not create an instance"
                });
                return default(T);
            }

            return plugin;
        }

        private T CreatePlugin<T>(string path, bool forClient)
        {
            var assembly = Assembly.Load(File.ReadAllBytes(path));
            if (forClient && assembly.ImageRuntimeVersion != "v2.0.50727")
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message =
                        $"False runtime version: {assembly.ImageRuntimeVersion} (expected: v2.0.50727). Please compile using the .Net Framework 3.5 or lower"
                });
                return default(T);
            }

            Type[] types;

            try
            {
                types = assembly.GetTypes();
                TestResultEntries.Add(new TestResultEntry {Message = "Assembly.GetTypes() succeed"});
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = "Assembly.GetTypes() failed"
                });

                return default(T);
            }

            Type type;
            try
            {
                type = types.First(x => x.IsSubclassOf(typeof(T)));
                TestResultEntries.Add(new TestResultEntry {Message = $"Class \"{type.Assembly}\" found"});
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = $"Could not find a class which inherits {typeof(T).FullName}"
                });
                return default(T);
            }

            T plugin;
            try
            {
                plugin = (T) Activator.CreateInstance(type);
                TestResultEntries.Add(new TestResultEntry {Message = "Instance successfully created"});
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = "Could not create an instance"
                });
                return default(T);
            }

            return plugin;
        }

        private List<T> CreatePlugins<T>(string path, bool forClient)
        {
            var assembly = Assembly.Load(File.ReadAllBytes(path));
            if (forClient && assembly.ImageRuntimeVersion != "v2.0.50727")
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Message =
                        $"False runtime version: {assembly.ImageRuntimeVersion} (expected: v2.0.50727). Please compile using the .Net Framework 3.5 or lower"
                });
                return null;
            }

            Type[] types;

            try
            {
                types = assembly.GetTypes();
                TestResultEntries.Add(new TestResultEntry { Message = "Assembly.GetTypes() succeed" });
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = "Assembly.GetTypes() failed"
                });

                return null;
            }

            List<Type> classTypes;
            try
            {
                classTypes = types.Where(x => x.IsSubclassOf(typeof(T)) && x.GetConstructor(Type.EmptyTypes) != null).ToList();
                TestResultEntries.Add(new TestResultEntry { Message = $"{classTypes.Count} classes found" });
            }
            catch (Exception ex)
            {
                TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    FullErrorText = ex.ToString(),
                    Message = $"Could not find a class which inherits {typeof(T).FullName}"
                });
                return null;
            }

            var plugins = new List<T>();
            foreach (var type in classTypes)
                try
                {
                    plugins.Add((T) Activator.CreateInstance(type));
                    TestResultEntries.Add(new TestResultEntry
                    {
                        Message = $"Instance of {type.Name} successfully created"
                    });
                }
                catch (Exception ex)
                {
                    TestResultEntries.Add(new TestResultEntry
                    {
                        Failed = true,
                        FullErrorText = ex.ToString(),
                        Message = $"Could not create an instance of {type.Name}"
                    });
                    return null;
                }

            return plugins;
        }
    }
}