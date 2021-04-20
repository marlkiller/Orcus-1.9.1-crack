using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;

namespace Orcus.Administration.Core.Build.Configuration
{
    public static class BuildConfigurationHelper
    {
        public const string BuildConfigurationFolderName = "configurations";

        public static List<BuildConfigurationInfo> LoadBuildConfigurations()
        {
            var directory = new DirectoryInfo(BuildConfigurationFolderName);
            if (!directory.Exists)
                return null;

            var types = GetRequiredTypes();
            var xmlSerializer = new XmlSerializer(typeof (BuildConfiguration), types.ToArray());
            var buildConfigurations = new List<BuildConfigurationInfo>();

            foreach (var fileInfo in directory.GetFiles("*.xml"))
            {
                try
                {
                    using (var streamReader = new StreamReader(fileInfo.FullName))
                    {
                        var buildConfiguration = (BuildConfiguration) xmlSerializer.Deserialize(streamReader);
                        buildConfigurations.Add(new BuildConfigurationInfo(buildConfiguration, fileInfo.FullName));
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return buildConfigurations;
        }

        public static void SaveBuildConfiguration(BuildConfiguration buildConfiguration, string fileName)
        {
            var directory = new FileInfo(fileName).Directory;
            if (!directory.Exists)
                directory.Create();

            var types = GetRequiredTypes();
            var xmlSerializer = new XmlSerializer(typeof(BuildConfiguration), types.ToArray());
            try
            {
                using (var streamWriter = new StreamWriter(fileName, false))
                    xmlSerializer.Serialize(streamWriter, buildConfiguration);
            }
            catch (Exception)
            {
                var file = new FileInfo(fileName);
                if (file.Exists && file.Length == 0)
                    file.Delete();

                throw;
            }
        }

        private static List<Type> GetRequiredTypes()
        {
            var types = new List<Type>(BuilderPropertyHelper.GetAllBuilderPropertyTypes());
            foreach (var clientPlugin in PluginManager.Current.LoadedPlugins.OfType<ClientPlugin>())
            {
                var providesProperties = clientPlugin.Plugin as IProvideEditableProperties;
                if (providesProperties != null)
                    types.AddRange(providesProperties.Properties.Select(x => x.PropertyType));

                var providesBuilderSettings = clientPlugin.Plugin as IProvideBuilderSettings;
                if (providesBuilderSettings != null)
                    types.AddRange(providesBuilderSettings.BuilderSettings.Select(x => x.BuilderProperty.GetType()));
            }

            foreach (var buildPlugin in PluginManager.Current.LoadedPlugins.OfType<BuildPlugin>())
            {
                var providesProperties = buildPlugin.Plugin as IProvideEditableProperties;
                if (providesProperties != null)
                    types.AddRange(providesProperties.Properties.Select(x => x.PropertyType));

                var providesBuilderSettings = buildPlugin.Plugin as IProvideBuilderSettings;
                if (providesBuilderSettings != null)
                    types.AddRange(providesBuilderSettings.BuilderSettings.Select(x => x.BuilderProperty.GetType()));
            }

            return types;
        }
    }
}