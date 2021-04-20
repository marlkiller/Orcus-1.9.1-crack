using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Shared.Client;
using Orcus.Shared.Core;
using PluginInfo = Orcus.Plugins.PluginInfo;

namespace Orcus.Administration.Core.Build
{
    public static class BuilderExtensions
    {
        public static T GetBuilderProperty<T>(this IEnumerable<IBuilderProperty> builderProperties)
            where T : IBuilderProperty
        {
            return builderProperties.OfType<T>().First();
        }

        public static void ExecuteBuildPluginEvents<TBuilderEvent>(this IEnumerable<BuildPluginEvent> builderEvents,
            Action<TBuilderEvent> executeEvent) where TBuilderEvent : BuilderEvent
        {
            foreach (var buildEvent in builderEvents.Where(x => x.BuilderEvent.GetType() == typeof (TBuilderEvent)))
            {
                try
                {
                    executeEvent.Invoke((TBuilderEvent) buildEvent.BuilderEvent);
                }
                catch (Exception ex)
                {
                    throw new BuilderEventExecutionException(ex, buildEvent);
                }
            }
        }

        public static ClientSetting ToClientSetting(this IBuilderProperty builderProperty)
        {
            return SerializeInternal<ClientSetting>(builderProperty);
        }

        public static PluginSetting ToPluginSetting(this IBuilderProperty builderProperty, PluginInfo plugin)
        {
            var pluginSettings = SerializeInternal<PluginSetting>(builderProperty);
            pluginSettings.PluginId = plugin.Guid;
            return pluginSettings;
        }

        private static T SerializeInternal<T>(IBuilderProperty builderProperty) where T : ClientSetting, new()
        {
            var propertyType = builderProperty.GetType();

            var settings = new T
            {
                Properties = new List<PropertyNameValue>(),
                SettingsType = propertyType.GetClientSettingTypeName()
            };

            foreach (var propertyInfo in propertyType.GetProperties())
            {
                if (propertyInfo.GetCustomAttribute(typeof(XmlIgnoreAttribute)) != null)
                    continue;

                settings.Properties.Add(new PropertyNameValue
                {
                    Name = propertyInfo.Name,
                    Value = propertyInfo.GetValue(builderProperty)
                });
            }

            return settings;
        }
    }

    public class BuilderEventExecutionException : Exception
    {
        public BuildPluginEvent BuildPluginEvent { get; }

        public BuilderEventExecutionException(Exception exception, BuildPluginEvent buildPluginEvent)
            : base(
                $"Exception occurred when executing builder plugin \"{buildPluginEvent.BuildPlugin.PluginInfo.Name}\" on event {buildPluginEvent.BuilderEvent}",
                exception)
        {
            BuildPluginEvent = buildPluginEvent;
        }
    }
}